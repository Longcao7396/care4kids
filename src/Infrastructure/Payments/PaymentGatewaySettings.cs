namespace GiveAID.Infrastructure.Payments;

public class PaymentGatewaySettings
{
    public string Type { get; set; } = "Mock"; // "Stripe" or "Mock"
    public string StripeSecretKey { get; set; } = string.Empty;
    public string StripeWebhookSecret { get; set; } = string.Empty;
}
