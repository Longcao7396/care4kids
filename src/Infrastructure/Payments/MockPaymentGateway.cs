using GiveAID.Application.Services;

namespace GiveAID.Infrastructure.Payments;

public class MockPaymentGateway : IPaymentGateway
{
    public string GatewayName => "Mock";

    public Task<PaymentIntentResult> CreatePaymentIntentAsync(long amount, string currency, int donationId, string email)
    {
        var transactionId = $"mock_{Guid.NewGuid():N}";
        return Task.FromResult(new PaymentIntentResult
        {
            Success = true,
            ClientSecret = transactionId,
            TransactionId = transactionId,
            ErrorMessage = null
        });
    }

    public Task<WebhookVerificationResult> VerifyWebhookAsync(string payload, string signature)
    {
        return Task.FromResult(new WebhookVerificationResult
        {
            Valid = true,
            EventType = "payment_intent.succeeded",
            EventId = Guid.NewGuid().ToString(),
            TransactionId = $"mock_{Guid.NewGuid():N}",
            RawPayload = payload
        });
    }

    public Task<PaymentStatusResult> GetPaymentStatusAsync(string transactionId)
    {
        return Task.FromResult(new PaymentStatusResult
        {
            Success = true,
            Status = "succeeded"
        });
    }

    public Task<RefundResult> ProcessRefundAsync(string transactionId, long? amount = null)
    {
        return Task.FromResult(new RefundResult
        {
            Success = true,
            RefundId = $"refund_{Guid.NewGuid():N}"
        });
    }
}
