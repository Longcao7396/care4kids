namespace GiveAID.Application.Services;

/// <summary>
/// Interface for payment gateway operations.
/// Implementation provided by Infrastructure layer.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Creates a payment intent with the payment gateway.
    /// </summary>
    Task<PaymentIntentResult> CreatePaymentIntentAsync(long amount, string currency, int donationId, string email);

    /// <summary>
    /// Verifies a webhook signature and returns the event payload.
    /// </summary>
    Task<WebhookVerificationResult> VerifyWebhookAsync(string payload, string signature);

    /// <summary>
    /// Retrieves the status of a payment by transaction ID.
    /// </summary>
    Task<PaymentStatusResult> GetPaymentStatusAsync(string transactionId);

    /// <summary>
    /// Processes a refund for a completed donation.
    /// </summary>
    Task<RefundResult> ProcessRefundAsync(string transactionId, long? amount = null);
}

/// <summary>
/// Result of creating a payment intent.
/// </summary>
public class PaymentIntentResult
{
    public bool Success { get; set; }
    public string? ClientSecret { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of webhook verification.
/// </summary>
public class WebhookVerificationResult
{
    public bool Valid { get; set; }
    public string? EventType { get; set; }
    public string? EventId { get; set; }
    public string? TransactionId { get; set; }
    public string? RawPayload { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of payment status query.
/// </summary>
public class PaymentStatusResult
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of refund processing.
/// </summary>
public class RefundResult
{
    public bool Success { get; set; }
    public string? RefundId { get; set; }
    public string? ErrorMessage { get; set; }
}
