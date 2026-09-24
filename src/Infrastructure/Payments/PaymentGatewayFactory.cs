using GiveAID.Application.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace GiveAID.Infrastructure.Payments;

public class PaymentGatewayFactory : IPaymentGateway
{
    private readonly IPaymentGateway _stripeGateway;
    private readonly IPaymentGateway _mockGateway;
    private readonly string _activeGatewayType;

    public PaymentGatewayFactory(
        IOptions<PaymentGatewaySettings> settings,
        ILogger<StripePaymentGateway> stripeLogger)
    {
        _activeGatewayType = settings.Value.Type;
        _mockGateway = new MockPaymentGateway();
        _stripeGateway = new StripePaymentGateway(settings, stripeLogger);
    }

    public string GatewayName => _activeGatewayType;

    private IPaymentGateway GetPaymentGatewayInternal()
    {
        return _activeGatewayType.ToLowerInvariant() switch
        {
            "stripe" => _stripeGateway,
            "mock" => _mockGateway,
            _ => _mockGateway
        };
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(long amount, string currency, int donationId, string email)
    {
        return await GetPaymentGatewayInternal().CreatePaymentIntentAsync(amount, currency, donationId, email);
    }

    public async Task<WebhookVerificationResult> VerifyWebhookAsync(string payload, string signature)
    {
        return await GetPaymentGatewayInternal().VerifyWebhookAsync(payload, signature);
    }

    public async Task<PaymentStatusResult> GetPaymentStatusAsync(string transactionId)
    {
        return await GetPaymentGatewayInternal().GetPaymentStatusAsync(transactionId);
    }

    public async Task<RefundResult> ProcessRefundAsync(string transactionId, long? amount = null)
    {
        return await GetPaymentGatewayInternal().ProcessRefundAsync(transactionId, amount);
    }
}
