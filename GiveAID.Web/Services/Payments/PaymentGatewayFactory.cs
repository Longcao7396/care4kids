using System;
using System.Configuration;
using System.Web.Configuration;

namespace GiveAID.Web.Services.Payments
{
    /// <summary>
    /// Factory that resolves the active payment gateway from configuration.
    ///
    /// Configuration:
    ///   PaymentGateway__Default  — gateway name: "stripe" | "vnpay" | "momo" | "mock"
    ///                              Defaults to "mock" if not set.
    ///   PaymentGateway__Enabled  — "true" | "false"  (default: "false")
    ///
    /// VNPay and MoMo are stub implementations that log a "not implemented" error.
    /// To enable them, implement VNPayPaymentGateway and MoMoPaymentGateway following
    /// the same pattern as StripePaymentGateway, then add the case here.
    /// </summary>
    public static class PaymentGatewayFactory
    {
        private static IPaymentGateway _singleton;
        private static readonly object _lock = new object();

        /// <summary>
        /// Returns the currently configured payment gateway instance (singleton).
        /// </summary>
        public static IPaymentGateway Current
        {
            get
            {
                if (_singleton != null) return _singleton;

                lock (_lock)
                {
                    if (_singleton != null) return _singleton;

                    var name = (ConfigurationManager.AppSettings["PaymentGateway__Default"] ?? "mock").Trim().ToLowerInvariant();
                    var enabled = string.Equals(
                        ConfigurationManager.AppSettings["PaymentGateway__Enabled"],
                        "true", StringComparison.OrdinalIgnoreCase);

                    if (!enabled)
                    {
                        // Return a disabled stub that always fails — better than null
                        _singleton = new DisabledPaymentGateway();
                        return _singleton;
                    }

                    _singleton = CreateGateway(name);
                    return _singleton;
                }
            }
        }

        /// <summary>
        /// Returns the configured gateway name.
        /// </summary>
        public static string CurrentGatewayName
        {
            get
            {
                return (ConfigurationManager.AppSettings["PaymentGateway__Default"] ?? "mock").Trim().ToLowerInvariant();
            }
        }

        /// <summary>
        /// Returns whether the gateway is enabled.
        /// </summary>
        public static bool IsEnabled
        {
            get
            {
                return string.Equals(
                    ConfigurationManager.AppSettings["PaymentGateway__Enabled"],
                    "true", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Returns the Stripe publishable key (pk_test_... or pk_live_...) for
        /// the frontend's use. Returns null if Stripe is not the active gateway.
        /// </summary>
        public static string StripePublishableKey
        {
            get
            {
                if (!string.Equals(CurrentGatewayName, "stripe", StringComparison.OrdinalIgnoreCase))
                    return null;
                return ConfigurationManager.AppSettings["Stripe__PublishableKey"]
                    ?? ConfigurationManager.AppSettings["Stripe:PublishableKey"];
            }
        }

        private static IPaymentGateway CreateGateway(string name)
        {
            switch (name)
            {
                case "stripe":
                    return new StripePaymentGateway();

                case "mock":
                    return new MockPaymentGateway();

                case "vnpay":
                    // TODO: implement VNPayPaymentGateway and uncomment
                    // return new VNPayPaymentGateway();
                    LogNotImplemented("VNPay", "VNPayPaymentGateway");
                    return new DisabledPaymentGateway();

                case "momo":
                    // TODO: implement MoMoPaymentGateway and uncomment
                    // return new MoMoPaymentGateway();
                    LogNotImplemented("MoMo", "MoMoPaymentGateway");
                    return new DisabledPaymentGateway();

                default:
                    System.Diagnostics.Debug.WriteLine(
                        $"[PaymentGatewayFactory] Unknown gateway '{name}', defaulting to MockPaymentGateway.");
                    return new MockPaymentGateway();
            }
        }

        private static void LogNotImplemented(string gateway, string className)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[PaymentGatewayFactory] {gateway} gateway is configured but {className} " +
                "is not yet implemented. Using disabled stub. " +
                $"Implement {className} following the StripePaymentGateway pattern, " +
                $"then add the case to PaymentGatewayFactory.CreateGateway().");
        }
    }

    /// <summary>
    /// Stub gateway returned when the configured gateway is not yet implemented
    /// or when PaymentGateway__Enabled != "true".
    /// </summary>
    internal class DisabledPaymentGateway : IPaymentGateway
    {
        public string Name => "disabled";

        public System.Threading.Tasks.Task<PaymentIntentResult> CreatePaymentIntent(
            decimal amount, string currency, string description,
            System.Collections.Generic.Dictionary<string, string> metadata)
        {
            return System.Threading.Tasks.Task.FromResult(new PaymentIntentResult
            {
                Success = false,
                ErrorCode = "GATEWAY_DISABLED",
                ErrorMessage = "No payment gateway is enabled. Set PaymentGateway__Default and PaymentGateway__Enabled in Web.config."
            });
        }

        public System.Threading.Tasks.Task<PaymentVerificationResult> VerifyWebhook(
            string rawBody, System.Collections.Generic.IDictionary<string, string> headers)
        {
            return System.Threading.Tasks.Task.FromResult(new PaymentVerificationResult
            {
                SignatureValid = false,
                ErrorMessage = "No payment gateway is enabled."
            });
        }

        public System.Threading.Tasks.Task<RefundResult> Refund(string transactionId, decimal amount)
        {
            return System.Threading.Tasks.Task.FromResult(new RefundResult
            {
                Success = false,
                ErrorMessage = "No payment gateway is enabled."
            });
        }
    }
}
