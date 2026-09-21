using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using GiveAID.Web.Data;
using GiveAID.Web.Models;
using GiveAID.Web.Helpers;
using GiveAID.Web.Services.Payments;

namespace GiveAID.Web.Controllers
{
    /// <summary>
    /// Admin endpoints for payment gateway management, refunds, and webhook audit logs.
    /// All endpoints require Admin or SuperAdmin role.
    /// </summary>
    [RoutePrefix("api/admin/payments")]
    [JwtAuthorize(Roles = "SuperAdmin,Admin")]
    public class AdminPaymentsController : ApiController
    {
        private readonly GiveAIDContext _context;

        public AdminPaymentsController()
        {
            _context = new GiveAIDContext();
        }

        // GET: api/admin/payments/gateways
        // Returns the list of available payment gateways with their config status.
        [HttpGet]
        [Route("gateways")]
        public IHttpActionResult GetGateways()
        {
            try
            {
                var defaultGateway = PaymentGatewayFactory.CurrentGatewayName;
                var isEnabled     = PaymentGatewayFactory.IsEnabled;
                var stripeKey     = PaymentGatewayFactory.StripePublishableKey;

                var gateways = new[]
                {
                    new
                    {
                        name        = "stripe",
                        displayName = "Stripe",
                        enabled     = isEnabled && defaultGateway == "stripe",
                        mode        = IsLiveMode("stripe") ? "live" : "test",
                        hasApiKey   = !string.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings["Stripe__ApiKey"]),
                        hasWebhookSecret = !string.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings["Stripe__WebhookSecret"]),
                        publishableKey = stripeKey ?? "",
                        docsUrl     = "https://stripe.com/docs"
                    },
                    new
                    {
                        name        = "vnpay",
                        displayName = "VNPay",
                        enabled     = false,  // Not implemented yet
                        mode        = "",
                        hasApiKey   = false,
                        hasWebhookSecret = false,
                        publishableKey = "",
                        docsUrl     = "https://sandbox.vnpayment.vn/apis/docs/huong-dan-tich-hop/"
                    },
                    new
                    {
                        name        = "momo",
                        displayName = "MoMo",
                        enabled     = false,  // Not implemented yet
                        mode        = "",
                        hasApiKey   = false,
                        hasWebhookSecret = false,
                        publishableKey = "",
                        docsUrl     = "https://developers.momo.vn/"
                    },
                    new
                    {
                        name        = "mock",
                        displayName = "Mock (Demo)",
                        enabled     = isEnabled && defaultGateway == "mock",
                        mode        = "test",
                        hasApiKey   = true,
                        hasWebhookSecret = true,
                        publishableKey = "",
                        docsUrl     = ""
                    }
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        activeGateway = defaultGateway,
                        globalEnabled = isEnabled,
                        gateways
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/admin/payments/webhook-logs
        // Returns paginated webhook event log.
        // Supports ?gateway=stripe&status=Processed&page=1&pageSize=50
        [HttpGet]
        [Route("webhook-logs")]
        public IHttpActionResult GetWebhookLogs(
            string gateway = null,
            string status = null,
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                var query = _context.WebhookLogs.AsQueryable();

                if (!string.IsNullOrWhiteSpace(gateway))
                    query = query.Where(w => w.Gateway == gateway);
                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(w => w.ProcessingStatus == status);

                var total = query.Count();
                var logs = query
                    .OrderByDescending(w => w.ReceivedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(w => new
                    {
                        w.WebhookLogId,
                        w.Gateway,
                        w.EventType,
                        w.EventId,
                        w.SignatureValid,
                        w.ProcessingStatus,
                        w.ErrorMessage,
                        w.DonationTransactionId,
                        w.DonationId,
                        w.ReceivedAt,
                        w.ProcessedAt
                    })
                    .ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        items = logs,
                        total,
                        page,
                        pageSize,
                        totalPages = (int)Math.Ceiling((double)total / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/admin/payments/webhook-logs/{id}
        // Returns a single webhook log entry including the raw payload.
        [HttpGet]
        [Route("webhook-logs/{id:long}")]
        public IHttpActionResult GetWebhookLogById(long id)
        {
            var log = _context.WebhookLogs.Find(id);
            if (log == null) return NotFound();

            return Ok(new ApiResponse
            {
                Success = true,
                Data = new
                {
                    log.WebhookLogId,
                    log.Gateway,
                    log.EventType,
                    log.EventId,
                    log.RawPayload,
                    log.Signature,
                    log.SignatureValid,
                    log.ProcessingStatus,
                    log.ErrorMessage,
                    log.DonationTransactionId,
                    log.DonationId,
                    log.ReceivedAt,
                    log.ProcessedAt
                }
            });
        }

        // POST: api/admin/payments/test-webhook
        // Simulates a test webhook from the active gateway.
        // Only works when the mock gateway is active.
        [HttpPost]
        [Route("test-webhook")]
        public async Task<IHttpActionResult> TestWebhook([FromBody] TestWebhookRequest request)
        {
            try
            {
                var gatewayName = PaymentGatewayFactory.CurrentGatewayName;

                if (gatewayName != "mock")
                {
                    return BadRequest(
                        "Test webhook simulation is only available when " +
                        "PaymentGateway__Default=mock. For real gateway testing, " +
                        "use Stripe CLI: stripe listen --forward-to localhost:44300/api/donations/webhook");
                }

                // Look up the donation
                if (!request.DonationId.HasValue && string.IsNullOrWhiteSpace(request.TransactionId))
                    return BadRequest("Provide either DonationId or TransactionId.");

                Donation donation = null;
                if (request.DonationId.HasValue)
                    donation = _context.Donations.Find(request.DonationId.Value);
                else if (!string.IsNullOrWhiteSpace(request.TransactionId))
                    donation = _context.Donations
                        .FirstOrDefault(d => d.TransactionId == request.TransactionId
                                          || d.GatewayTransactionId == request.TransactionId);

                if (donation == null)
                    return BadRequest("Donation not found.");

                var eventType = request.EventType ?? "mock.payment.succeeded";
                var payload = MockPaymentGateway.SimulateWebhook(
                    eventType,
                    donation.GatewayTransactionId ?? donation.TransactionId,
                    donation.Amount);

                // Process the mock webhook through the gateway
                var gateway = PaymentGatewayFactory.Current;
                var headers = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["X-Gateway-Signature"] = "mock-test-signature"
                };

                var result = await gateway.VerifyWebhook(payload, headers);

                // Log the test event
                var log = new WebhookLog
                {
                    Gateway = "mock",
                    EventType = result.EventType,
                    EventId = result.EventId,
                    RawPayload = payload.Length > 4000 ? payload.Substring(0, 4000) : payload,
                    SignatureValid = true,
                    ProcessingStatus = "Processed",
                    DonationTransactionId = donation.TransactionId,
                    DonationId = donation.DonationId,
                    ReceivedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };
                _context.WebhookLogs.Add(log);

                // Apply the status change
                await ApplyDonationStatusChange(donation, result);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = $"Test webhook ({eventType}) processed. Donation {donation.DonationId} is now {donation.PaymentStatus}.",
                    Data = new
                    {
                        donation.DonationId,
                        donation.PaymentStatus,
                        eventId = result.EventId
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/admin/payments/{donationId}/refund
        // Issues a refund for a completed donation via the payment gateway.
        [HttpPost]
        [Route("{donationId:int}/refund")]
        public async Task<IHttpActionResult> RefundDonation(int donationId, [FromBody] RefundRequest request)
        {
            try
            {
                var donation = _context.Donations.Find(donationId);
                if (donation == null) return NotFound();

                if (donation.PaymentStatus != "Completed" && donation.PaymentStatus != "Refunded")
                    return BadRequest($"Cannot refund a donation with status '{donation.PaymentStatus}'. Only Completed donations can be refunded.");

                var txnId = donation.GatewayTransactionId ?? donation.TransactionId;

                var gateway = PaymentGatewayFactory.Current;
                if (gateway.Name == "disabled")
                    return BadRequest("No payment gateway is enabled. Cannot process refund.");

                var refundAmount = request?.Amount > 0 ? request.Amount : donation.Amount;
                if (refundAmount > donation.Amount)
                    return BadRequest("Refund amount cannot exceed the original donation amount.");

                var result = await gateway.Refund(txnId, refundAmount ?? 0m);

                if (!result.Success)
                {
                    // Log the failed refund attempt
                    _context.WebhookLogs.Add(new WebhookLog
                    {
                        Gateway = gateway.Name,
                        EventType = "refund.failed",
                        EventId = $"refund_attempt_{donationId}_{DateTime.UtcNow:yyyyMMddHHmmss}",
                        RawPayload = $"{{\"donationId\":{donationId},\"amount\":{refundAmount}}}",
                        SignatureValid = true,
                        ProcessingStatus = "Failed",
                        ErrorMessage = result.ErrorMessage,
                        DonationId = donationId,
                        DonationTransactionId = txnId,
                        ReceivedAt = DateTime.UtcNow,
                        ProcessedAt = DateTime.UtcNow
                    });
                    _context.SaveChanges();

                    return Content(HttpStatusCode.BadRequest, new ApiResponse
                    {
                        Success = false,
                        Message = "Refund failed at the payment gateway.",
                        Data = new { result.ErrorCode, result.ErrorMessage }
                    });
                }

                // Update donation status
                var previousStatus = donation.PaymentStatus;
                donation.PaymentStatus = refundAmount >= donation.Amount ? "Refunded" : "PartiallyRefunded";
                donation.PaymentConfirmedAt = DateTime.UtcNow;

                // Decrement raised amounts proportionally
                var updatedAt = DateTime.Now;
                _context.Database.ExecuteSqlCommand(
                    "UPDATE Causes SET raised_amount = COALESCE(raised_amount, 0) - @p0, updated_at = @p1 WHERE cause_id = @p2",
                    refundAmount, updatedAt, donation.CauseId);
                if (donation.CampaignId.HasValue)
                {
                    _context.Database.ExecuteSqlCommand(
                        "UPDATE Campaigns SET raised_amount = COALESCE(raised_amount, 0) - @p0, updated_at = @p1 WHERE campaign_id = @p2",
                        refundAmount, updatedAt, donation.CampaignId.Value);
                }

                // Log the refund
                _context.WebhookLogs.Add(new WebhookLog
                {
                    Gateway = gateway.Name,
                    EventType = "charge.refunded",
                    EventId = result.RefundId,
                    RawPayload = $"{{\"donationId\":{donationId},\"amount\":{refundAmount},\"refundId\":\"{result.RefundId}\"}}",
                    SignatureValid = true,
                    ProcessingStatus = "Processed",
                    DonationId = donationId,
                    DonationTransactionId = txnId,
                    ReceivedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                });

                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = $"Refund of {refundAmount:N0} VND processed successfully.",
                    Data = new
                    {
                        donation.DonationId,
                        previousStatus,
                        donation.PaymentStatus,
                        refundId = result.RefundId,
                        refundAmount
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/admin/payments/{donationId}/retry-webhook
        // Re-processes a webhook log entry by its ID (useful when a prior webhook
        // failed due to a bug that has since been fixed).
        [HttpPost]
        [Route("{donationId:int}/retry-webhook")]
        public async Task<IHttpActionResult> RetryWebhook(int donationId, [FromBody] RetryWebhookRequest request)
        {
            if (!request.LogId.HasValue)
                return BadRequest("logId is required.");

            var log = _context.WebhookLogs.Find(request.LogId.Value);
                if (log == null) return BadRequest("Webhook log not found.");
                if (log.DonationId != donationId)
                    return BadRequest("Log entry does not belong to this donation.");

                var donation = _context.Donations.Find(donationId);
                if (donation == null) return BadRequest("Donation not found.");

            var headers = new System.Collections.Generic.Dictionary<string, string>
            {
                ["X-Gateway-Signature"] = log.Signature ?? ""
            };

            var gateway = PaymentGatewayFactory.Current;
            var result  = await gateway.VerifyWebhook(log.RawPayload ?? "{}", headers);

            log.ReceivedAt = DateTime.UtcNow; // update timestamp
            log.ProcessedAt = null;

            await ApplyDonationStatusChange(donation, result);

            log.ProcessingStatus = "Processed";
            log.ErrorMessage = null;
            log.SignatureValid = result.SignatureValid;
            log.ProcessedAt = DateTime.UtcNow;

            _context.SaveChanges();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = $"Webhook log {log.WebhookLogId} re-processed. Donation status: {donation.PaymentStatus}",
                Data = new { donation.DonationId, donation.PaymentStatus }
            });
        }

        // ─── Private helpers ──────────────────────────────────────────────────────

        private Task ApplyDonationStatusChange(Donation donation, PaymentVerificationResult result)
        {
            if (donation == null || result == null) return Task.CompletedTask;

            var now = DateTime.UtcNow;

            switch ((result.Status ?? "").ToLowerInvariant())
            {
                case "succeeded":
                case "completed":
                case "paid":
                    if (donation.PaymentStatus != "Completed")
                    {
                        donation.PaymentStatus = "Completed";
                        donation.PaymentConfirmedAt = now;
                        donation.GatewayTransactionId = result.TransactionId ?? donation.GatewayTransactionId;

                        var updatedAt = DateTime.Now;
                        _context.Database.ExecuteSqlCommand(
                            "UPDATE Causes SET raised_amount = COALESCE(raised_amount, 0) + @p0, updated_at = @p1 WHERE cause_id = @p2",
                            donation.Amount, updatedAt, donation.CauseId);
                        if (donation.CampaignId.HasValue)
                        {
                            _context.Database.ExecuteSqlCommand(
                                "UPDATE Campaigns SET raised_amount = COALESCE(raised_amount, 0) + @p0, updated_at = @p1 WHERE campaign_id = @p2",
                                donation.Amount, updatedAt, donation.CampaignId.Value);
                        }
                    }
                    break;

                case "failed":
                case "declined":
                    donation.PaymentStatus = "Failed";
                    break;

                case "refunded":
                    donation.PaymentStatus = "Refunded";
                    // Decrement raised amounts
                    _context.Database.ExecuteSqlCommand(
                        "UPDATE Causes SET raised_amount = COALESCE(raised_amount, 0) - @p0, updated_at = GETDATE() WHERE cause_id = @p1",
                        donation.Amount, donation.CauseId);
                    if (donation.CampaignId.HasValue)
                    {
                        _context.Database.ExecuteSqlCommand(
                            "UPDATE Campaigns SET raised_amount = COALESCE(raised_amount, 0) - @p0, updated_at = GETDATE() WHERE campaign_id = @p1",
                            donation.Amount, donation.CampaignId.Value);
                    }
                    break;

                default:
                    // Unknown status — log but don't change donation
                    System.Diagnostics.Debug.WriteLine(
                        $"[AdminPayments] Unknown webhook status '{result.Status}' for donation {donation.DonationId}");
                    break;
            }

            return Task.CompletedTask;
        }

        private bool IsLiveMode(string gateway)
        {
            if (gateway == "stripe")
            {
                var key = System.Configuration.ConfigurationManager.AppSettings["Stripe__ApiKey"] ?? "";
                return key.StartsWith("sk_live_", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }

    public class TestWebhookRequest
    {
        public int? DonationId { get; set; }
        public string TransactionId { get; set; }
        public string EventType { get; set; } = "mock.payment.succeeded";
    }

    public class RefundRequest
    {
        public decimal? Amount { get; set; }
    }

    public class RetryWebhookRequest
    {
        public long? LogId { get; set; }
    }
}
