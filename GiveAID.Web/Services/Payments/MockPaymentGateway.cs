using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Web.Configuration;

namespace GiveAID.Web.Services.Payments
{
    /// <summary>
    /// Mock payment gateway for local development and demo environments.
    ///
    /// Configuration keys (add to Web.config / appSettings):
    ///   PaymentGateway__Default  — set to "mock" to activate
    ///   PaymentGateway__Enabled  — "true" (default false when not set)
    ///   MockGateway__DelayMs     — Simulated processing delay in ms (default 500)
    ///   MockGateway__FailRate    — Probability 0–1 that a payment fails (default 0)
    ///
    /// The mock gateway auto-approves every payment intent and provides a fake
    /// webhook simulation endpoint so the full donation flow can be tested
    /// end-to-end without any real payment processor credentials.
    /// </summary>
    public class MockPaymentGateway : IPaymentGateway
    {
        private readonly bool _enabled;
        private readonly int _delayMs;
        private readonly double _failRate;

        public string Name => "mock";

        public MockPaymentGateway()
            : this(
                  enabled: string.Equals(
                      WebConfigurationManager.AppSettings["PaymentGateway__Enabled"],
                      "true", StringComparison.OrdinalIgnoreCase)
                      && string.Equals(
                          WebConfigurationManager.AppSettings["PaymentGateway__Default"],
                          "mock", StringComparison.OrdinalIgnoreCase),
                  delayMs: int.TryParse(
                      WebConfigurationManager.AppSettings["MockGateway__DelayMs"], out var d) ? d : 500,
                  failRate: double.TryParse(
                      WebConfigurationManager.AppSettings["MockGateway__FailRate"], out var f) ? f : 0)
        { }

        public MockPaymentGateway(bool enabled, int delayMs, double failRate)
        {
            _enabled  = enabled;
            _delayMs  = Math.Max(0, delayMs);
            _failRate = failRate < 0 ? 0 : failRate > 1 ? 1 : failRate;
        }

        public async Task<PaymentIntentResult> CreatePaymentIntent(
            decimal amount,
            string currency,
            string description,
            Dictionary<string, string> metadata)
        {
            if (!_enabled)
                return new PaymentIntentResult
                {
                    Success = false,
                    ErrorMessage = "Mock gateway is disabled. Set PaymentGateway__Default=mock and PaymentGateway__Enabled=true in Web.config."
                };

            await Task.Delay(_delayMs);

            // Simulate random failure
            if (_failRate > 0 && new Random().NextDouble() < _failRate)
            {
                return new PaymentIntentResult
                {
                    Success = false,
                    ErrorCode = "card_declined",
                    ErrorMessage = "Your card was declined. (Mock)"
                };
            }

            var txnId = $"mock_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}".Substring(0, 40);

            return new PaymentIntentResult
            {
                Success = true,
                TransactionId = txnId,
                ClientSecret = $"mock_secret_{Guid.NewGuid():N}",
                GatewayTransactionId = txnId,
                Metadata = new Dictionary<string, string>
                {
                    ["gateway"] = "mock",
                    ["currency"] = currency ?? "vnd",
                    ["demo"] = "true"
                }
            };
        }

        public async Task<PaymentVerificationResult> VerifyWebhook(
            string rawBody,
            IDictionary<string, string> headers)
        {
            // Mock always trusts its own webhooks
            await Task.Delay(50);

            if (string.IsNullOrWhiteSpace(rawBody))
                return new PaymentVerificationResult
                {
                    SignatureValid = false,
                    ErrorMessage = "Empty webhook body"
                };

            var eventType = ExtractValue(rawBody, "type") ?? "mock.event";
            var eventId   = ExtractValue(rawBody, "id")    ?? Guid.NewGuid().ToString();
            var txnId     = ExtractValue(ExtractObject(rawBody, "data.object"), "id") ?? eventId;
            var status    = MapMockStatus(eventType);

            return new PaymentVerificationResult
            {
                SignatureValid = true,
                Gateway = "mock",
                EventType = eventType,
                EventId = eventId,
                TransactionId = txnId,
                RawPayload = rawBody.Length > 4000 ? rawBody.Substring(0, 4000) : rawBody,
                Status = status
            };
        }

        public async Task<RefundResult> Refund(string transactionId, decimal amount)
        {
            await Task.Delay(_delayMs / 2);

            return new RefundResult
            {
                Success = true,
                RefundId = $"mock_refund_{Guid.NewGuid():N}".Substring(0, 30),
                Status = "succeeded"
            };
        }

        // ─── Mock webhook simulator ────────────────────────────────────────────────
        // Call this from the webhook endpoint to simulate a gateway event.
        // Returns the JSON payload to store in WebhookLogs.

        public static string SimulateWebhook(string eventType, string transactionId, decimal amount)
        {
            var payload = $@"{{
  ""id"": ""evt_mock_{Guid.NewGuid():N}"",
  ""type"": ""{eventType}"",
  ""livemode"": false,
  ""created"": {DateTimeOffset.UtcNow.ToUnixTimeSeconds()},
  ""data"": {{
    ""object"": {{
      ""id"": ""{transactionId}"",
      ""amount"": {((long)amount).ToString()},
      ""amount_received"": {((long)amount).ToString()},
      ""currency"": ""vnd"",
      ""status"": ""succeeded"",
      ""metadata"": {{
        ""source"": ""GiveAID_MockGateway""
      }}
    }}
  }}
}}";
            return payload;
        }

        // ─── Private helpers ─────────────────────────────────────────────────────

        private static string MapMockStatus(string eventType)
        {
            switch (eventType)
            {
                case "payment_intent.succeeded":
                case "mock.payment.succeeded":
                    return "succeeded";
                case "payment_intent.payment_failed":
                case "mock.payment.failed":
                    return "failed";
                case "charge.refunded":
                case "mock.payment.refunded":
                    return "refunded";
                default:
                    return "unknown";
            }
        }

        private static string ExtractValue(string json, string key)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            var search = $"\"{key}\"";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return null;
            var colon = json.IndexOf(':', idx);
            if (colon < 0) return null;
            var start = json.Substring(colon + 1).TrimStart();
            if (start.StartsWith("\""))
            {
                var end = 1;
                while (end < start.Length)
                {
                    if (start[end] == '"' && start[end - 1] != '\\') break;
                    end++;
                }
                return start.Substring(1, end - 1);
            }
            var numEnd = 0;
            while (numEnd < start.Length && (char.IsDigit(start[numEnd]) || start[numEnd] == '.' || start[numEnd] == '-' || start[numEnd] == '+'))
                numEnd++;
            return numEnd > 0 ? start.Substring(0, numEnd) : null;
        }

        private static string ExtractObject(string json, string key)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            var search = $"\"{key}\"";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return null;
            var colon = json.IndexOf(':', idx);
            if (colon < 0) return null;
            var trimmed = json.Substring(colon + 1).TrimStart();
            if (!trimmed.StartsWith("{")) return null;
            var depth = 0;
            for (var i = 0; i < trimmed.Length; i++)
            {
                if (trimmed[i] == '{' || trimmed[i] == '[') depth++;
                else if (trimmed[i] == '}' || trimmed[i] == ']') { depth--; if (depth == 0) return trimmed.Substring(0, i + 1); }
            }
            return null;
        }
    }
}
