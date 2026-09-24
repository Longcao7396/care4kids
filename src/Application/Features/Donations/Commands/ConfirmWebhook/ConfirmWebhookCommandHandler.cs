using GiveAID.Application.Features.Donations.Commands.ConfirmWebhook;
using GiveAID.Application.Services;
using GiveAID.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GiveAID.Application.Features.Donations.Commands.ConfirmWebhook;

/// <summary>
/// Handler for ConfirmWebhookCommand.
/// L-04: Implements complete webhook handling with:
/// - Signature verification (via IPaymentGateway)
/// - Idempotency via event ID deduplication
/// - WebhookLog persistence for audit trail
/// - Domain state machine transitions for donations
/// </summary>
public class ConfirmWebhookCommandHandler : IRequestHandler<ConfirmWebhookCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IEmailSender _emailSender;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ConfirmWebhookCommandHandler> _logger;

    public ConfirmWebhookCommandHandler(
        IApplicationDbContext context,
        IPaymentGateway paymentGateway,
        IEmailSender emailSender,
        ICacheService cacheService,
        ILogger<ConfirmWebhookCommandHandler> logger)
    {
        _context = context;
        _paymentGateway = paymentGateway;
        _emailSender = emailSender;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<bool> Handle(ConfirmWebhookCommand request, CancellationToken cancellationToken)
    {
        // L-04: Verify webhook signature (Stripe validates timestamp + HMAC)
        var verification = await _paymentGateway.VerifyWebhookAsync(request.Payload, request.Signature);

        // L-04: Determine event ID for idempotency
        var eventId = verification.EventId ?? Guid.NewGuid().ToString();

        // L-04: Log the incoming webhook to WebhookLog table
        var webhookLog = new WebhookLog
        {
            Gateway = request.Gateway,
            EventType = verification.EventType ?? "unknown",
            EventId = eventId,
            RawPayload = verification.RawPayload ?? request.Payload,
            Signature = request.Signature,
            SignatureValid = verification.Valid,
            ProcessingStatus = "Processed",
            ReceivedAt = DateTime.UtcNow
        };
        _context.WebhookLogs.Add(webhookLog);

        if (!verification.Valid)
        {
            webhookLog.ProcessingStatus = "Failed";
            webhookLog.ErrorMessage = verification.ErrorMessage ?? "Invalid webhook signature";
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Stripe webhook signature invalid: {Error}", verification.ErrorMessage);
            return false;
        }

        // L-04: Idempotency check — have we already processed this event ID?
        // Stripe may retry webhooks; we must not process the same event twice.
        var existingLog = await _context.WebhookLogs
            .IgnoreQueryFilters()
            .Where(w => w.EventId == eventId && w.Gateway == request.Gateway && w.ProcessingStatus == "Processed")
            .Where(w => w.WebhookLogId != webhookLog.WebhookLogId) // exclude current
            .AnyAsync(cancellationToken);

        if (existingLog)
        {
            _logger.LogInformation(
                "Stripe webhook duplicate detected (idempotency). EventId={EventId} already processed. Skipping.",
                eventId);
            webhookLog.ProcessingStatus = "Duplicate";
            await _context.SaveChangesAsync(cancellationToken);
            return true; // Return 200 to Stripe — we handled it already
        }

        // L-04: Handle the event based on type
        var eventType = verification.EventType ?? "";
        var transactionId = verification.TransactionId ?? "";

        webhookLog.DonationTransactionId = transactionId;

        // Find the donation by transaction ID
        var donation = _context.Donations.FirstOrDefault(d =>
            d.GatewayTransactionId == transactionId);

        if (donation != null)
        {
            webhookLog.DonationId = donation.DonationId;
        }

        // L-04: Process based on event type
        if (eventType == "payment_intent.succeeded" || eventType == "charge.succeeded")
        {
            if (donation != null)
            {
                donation.MarkAsCompleted();

                // Update campaign raised amount
                if (donation.CampaignId.HasValue)
                {
                    var campaign = await _context.Campaigns.FindAsync(
                        new object[] { donation.CampaignId.Value }, cancellationToken);
                    if (campaign != null)
                    {
                        campaign.RaisedAmount += donation.Amount;
                    }
                }

                // Update cause raised amount
                var cause = await _context.Causes.FindAsync(
                    new object[] { donation.CauseId }, cancellationToken);
                if (cause != null)
                {
                    cause.RaisedAmount += donation.Amount;
                }

                _logger.LogInformation(
                    "Donation {DonationId} marked as Completed via webhook. EventId={EventId}",
                    donation.DonationId, eventId);
            }
            else
            {
                _logger.LogWarning(
                    "Webhook event {EventId} for transaction {TransactionId} — no matching donation found",
                    eventId, transactionId);
            }
        }
        else if (eventType == "payment_intent.payment_failed" || eventType == "charge.failed")
        {
            if (donation != null)
            {
                donation.MarkAsFailed();
                _logger.LogInformation(
                    "Donation {DonationId} marked as Failed via webhook. EventId={EventId}",
                    donation.DonationId, eventId);
            }
        }
        else if (eventType == "charge.refunded")
        {
            if (donation != null)
            {
                donation.MarkAsRefunded();
                _logger.LogInformation(
                    "Donation {DonationId} marked as Refunded via webhook. EventId={EventId}",
                    donation.DonationId, eventId);
            }
        }
        else
        {
            _logger.LogInformation(
                "Unhandled Stripe event type: {EventType}. EventId={EventId}",
                eventType, eventId);
            webhookLog.ProcessingStatus = "Ignored";
        }

        // L-04: Invalidate statistics cache after successful processing
        if (donation != null && (eventType.Contains("succeeded") || eventType.Contains("failed") || eventType.Contains("refunded")))
        {
            _cacheService.InvalidateStatistics();
        }

        webhookLog.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
