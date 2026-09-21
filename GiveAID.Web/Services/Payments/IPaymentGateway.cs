using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GiveAID.Web.Services.Payments
{
    /// <summary>
    /// Result of a CreatePaymentIntent call.
    /// </summary>
    public class PaymentIntentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }          // Internal/reference ID
        public string ClientSecret { get; set; }           // Client-facing secret (Stripe: pi_xxx_secret_xxx)
        public string GatewayTransactionId { get; set; }    // External gateway ID
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    /// <summary>
    /// Result of VerifyWebhook — confirms the webhook payload is authentic.
    /// </summary>
    public class PaymentVerificationResult
    {
        public bool SignatureValid { get; set; }
        public string Gateway { get; set; }               // "stripe", "vnpay", etc.
        public string EventType { get; set; }             // "payment_intent.succeeded", etc.
        public string EventId { get; set; }               // Unique event ID from gateway
        public string TransactionId { get; set; }         // Our internal TransactionId
        public string RawPayload { get; set; }            // Original body (truncated)
        public string Status { get; set; }               // "succeeded", "failed", "refunded", etc.
        public string ErrorMessage { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    /// <summary>
    /// Result of a Refund call.
    /// </summary>
    public class RefundResult
    {
        public bool Success { get; set; }
        public string RefundId { get; set; }
        public string Status { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Abstract payment gateway interface.
    /// Each concrete implementation (Stripe, VNPay, MoMo) handles its own
    /// API quirks, signature verification, and response parsing.
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>
        /// Unique gateway identifier: "stripe", "vnpay", "momo", "mock".
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Creates a payment intent / order with the gateway.
        /// Returns clientSecret so the frontend can confirm the payment via Stripe.js
        /// (or equivalent SDK for other gateways).
        /// </summary>
        Task<PaymentIntentResult> CreatePaymentIntent(
            decimal amount,
            string currency,
            string description,
            Dictionary<string, string> metadata);

        /// <summary>
        /// Verifies an incoming webhook: validates the signature, parses the event,
        /// and returns structured data about what happened.
        /// </summary>
        Task<PaymentVerificationResult> VerifyWebhook(
            string rawBody,
            IDictionary<string, string> headers);

        /// <summary>
        /// Refunds a previously completed transaction.
        /// </summary>
        Task<RefundResult> Refund(string transactionId, decimal amount);
    }
}
