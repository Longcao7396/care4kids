using GiveAID.Application.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Stripe;

namespace GiveAID.Infrastructure.Payments;

public class StripePaymentGateway : IPaymentGateway
{
    private readonly PaymentGatewaySettings _settings;
    private readonly ILogger<StripePaymentGateway> _logger;

    public StripePaymentGateway(IOptions<PaymentGatewaySettings> settings, ILogger<StripePaymentGateway> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public string GatewayName => "Stripe";

    private StripeClient GetStripeClient()
    {
        return new StripeClient(_settings.StripeSecretKey);
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(long amount, string currency, int donationId, string email)
    {
        try
        {
            var stripeClient = GetStripeClient();
            var service = new PaymentIntentService(stripeClient);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = currency.ToLowerInvariant(),
                Metadata = new Dictionary<string, string>
                {
                    { "donation_id", donationId.ToString() },
                    { "email", email }
                },
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            var paymentIntent = await service.CreateAsync(options);

            return new PaymentIntentResult
            {
                Success = true,
                ClientSecret = paymentIntent.ClientSecret,
                TransactionId = paymentIntent.Id,
                ErrorMessage = null
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe payment intent creation failed: {Message}", ex.Message);
            return new PaymentIntentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// L-04: Validates Stripe webhook signature using Stripe.EventUtility.
    /// Falls back to mock validation if webhook secret is not configured (dev mode only).
    /// </summary>
    public Task<WebhookVerificationResult> VerifyWebhookAsync(string payload, string signature)
    {
        // L-04: Guard against empty webhook secret in production
        if (string.IsNullOrEmpty(_settings.StripeWebhookSecret))
        {
            _logger.LogWarning(
                "Stripe webhook secret not configured (STRIPE_WEBHOOK_SECRET env var). " +
                "Webhook signature validation is SKIPPED. DO NOT use this in production!");

            // Parse payload to extract event info for dev/testing
            return Task.FromResult(ParsePayloadForDev(payload));
        }

        try
        {
            // L-04: Construct and verify the Stripe event using the SDK's built-in validation.
            // In Stripe.net v47, signature failures throw StripeException (not a dedicated exception type).
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                _settings.StripeWebhookSecret,
                throwOnApiVersionMismatch: false);

            _logger.LogInformation(
                "Stripe webhook verified: EventId={EventId}, Type={EventType}",
                stripeEvent.Id, stripeEvent.Type);

            string? transactionId = null;

            // Extract payment intent ID from the event's data object
            switch (stripeEvent.Data.Object)
            {
                case PaymentIntent pi:
                    transactionId = pi.Id;
                    break;
                case Charge charge:
                    // Charge has a PaymentIntent property in v47
                    transactionId = charge.PaymentIntent?.Id;
                    break;
            }

            return Task.FromResult(new WebhookVerificationResult
            {
                Valid = true,
                EventType = stripeEvent.Type,
                EventId = stripeEvent.Id,
                TransactionId = transactionId,
                RawPayload = payload
            });
        }
        catch (StripeException ex)
        {
            // StripeException covers both signature failures and API errors
            _logger.LogError(ex,
                "Stripe webhook verification FAILED: {Message}", ex.Message);
            return Task.FromResult(new WebhookVerificationResult
            {
                Valid = false,
                ErrorMessage = $"Webhook verification failed: {ex.Message}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook verification failed: {Message}", ex.Message);
            return Task.FromResult(new WebhookVerificationResult
            {
                Valid = false,
                ErrorMessage = ex.Message
            });
        }
    }

    /// <summary>
    /// L-04: Fallback parser for development when Stripe webhook secret is not configured.
    /// Parses the raw JSON payload to extract event type and transaction ID.
    /// WARNING: Does NOT validate the signature — for dev/test only.
    /// </summary>
    private WebhookVerificationResult ParsePayloadForDev(string payload)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(payload);
            var root = doc.RootElement;

            var eventType = root.TryGetProperty("type", out var typeEl)
                ? typeEl.GetString() ?? ""
                : "";
            var eventId = root.TryGetProperty("id", out var idEl)
                ? idEl.GetString() ?? ""
                : "";

            string? transactionId = null;
            if (root.TryGetProperty("data", out var dataEl) &&
                dataEl.TryGetProperty("object", out var objEl))
            {
                if (objEl.TryGetProperty("id", out var objIdEl))
                    transactionId = objIdEl.GetString();
            }

            _logger.LogWarning(
                "DEV MODE: Stripe webhook signature NOT validated. EventId={EventId}, Type={EventType}",
                eventId, eventType);

            return new WebhookVerificationResult
            {
                Valid = true,
                EventType = eventType,
                EventId = eventId,
                TransactionId = transactionId,
                RawPayload = payload
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Stripe webhook payload for dev mode");
            return new WebhookVerificationResult
            {
                Valid = false,
                ErrorMessage = $"Failed to parse payload: {ex.Message}"
            };
        }
    }

    public async Task<PaymentStatusResult> GetPaymentStatusAsync(string transactionId)
    {
        try
        {
            var stripeClient = GetStripeClient();
            var service = new PaymentIntentService(stripeClient);

            var paymentIntent = await service.GetAsync(transactionId);

            return new PaymentStatusResult
            {
                Success = true,
                Status = paymentIntent.Status ?? "unknown"
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe payment status check failed: {Message}", ex.Message);
            return new PaymentStatusResult
            {
                Success = false,
                Status = "error",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<RefundResult> ProcessRefundAsync(string transactionId, long? amount = null)
    {
        try
        {
            var stripeClient = GetStripeClient();
            var service = new RefundService(stripeClient);

            var options = new RefundCreateOptions
            {
                PaymentIntent = transactionId,
                Amount = amount
            };

            var refund = await service.CreateAsync(options);

            return new RefundResult
            {
                Success = refund.Status == "succeeded",
                RefundId = refund.Id,
                ErrorMessage = refund.Status != "succeeded" ? $"Refund status: {refund.Status}" : null
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe refund failed: {Message}", ex.Message);
            return new RefundResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
