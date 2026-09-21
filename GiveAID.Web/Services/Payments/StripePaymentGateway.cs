using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace GiveAID.Web.Services.Payments
{
    /// <summary>
    /// Stripe implementation of <see cref="IPaymentGateway"/>.
    /// Uses Stripe's REST API directly via HttpClient (no heavy SDK dependency).
    ///
    /// Configuration keys (add to Web.config / appSettings):
    ///   Stripe__ApiKey          — Secret key (sk_test_... or sk_live_...)
    ///   Stripe__WebhookSecret   — whsec_... from Stripe dashboard
    ///   Stripe__PublishableKey  — pk_test_... (for frontend; also returned in API response)
    ///   Stripe__Mode            — "test" | "live"  (default: "test")
    ///   Stripe__ApiBase         — https://api.stripe.com  (can be overridden for proxies)
    /// </summary>
    public class StripePaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _webhookSecret;
        private readonly string _publishableKey;
        private readonly bool _enabled;

        public string Name => "stripe";

        public StripePaymentGateway()
            : this(
                  apiKey: WebConfigurationManager.AppSettings["Stripe__ApiKey"]
                       ?? WebConfigurationManager.AppSettings["Stripe:ApiKey"],
                  webhookSecret: WebConfigurationManager.AppSettings["Stripe__WebhookSecret"],
                  publishableKey: WebConfigurationManager.AppSettings["Stripe__PublishableKey"],
                  enabled: string.Equals(
                      WebConfigurationManager.AppSettings["PaymentGateway__Enabled"],
                      "true", StringComparison.OrdinalIgnoreCase))
        { }

        // Constructor injectable for unit testing
        public StripePaymentGateway(string apiKey, string webhookSecret, string publishableKey, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException(nameof(apiKey), "Stripe API key is not configured. Add Stripe__ApiKey to Web.config appSettings.");

            _apiKey = apiKey;
            _webhookSecret = webhookSecret ?? "";
            _publishableKey = publishableKey ?? "";
            _enabled = enabled;

            _http = new HttpClient
            {
                BaseAddress = new Uri(
                    WebConfigurationManager.AppSettings["Stripe__ApiBase"]
                    ?? "https://api.stripe.com/v1/"),
                Timeout = TimeSpan.FromSeconds(20)
            };

            // Stripe uses HTTP Basic Auth: authorization = "Bearer sk_..."
            var authBytes = Encoding.UTF8.GetBytes($"{_apiKey}:");
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(authBytes));
        }

        /// <inheritdoc />
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
                    ErrorMessage = "Stripe gateway is disabled. Set PaymentGateway__Enabled=true in Web.config."
                };

            try
            {
                // Stripe expects amount in smallest currency unit (VND has 0 decimals).
                // amount is already in VND, so pass as-is.
                var form = new Dictionary<string, string>
                {
                    ["amount"]   = ((long)amount).ToString(),
                    ["currency"] = (currency ?? "vnd").ToLowerInvariant(),
                    ["description"] = description ?? "",
                    ["automatic_payment_methods[enabled]"] = "true",
                    // Metadata fields
                    ["metadata[source]"] = "GiveAID_WebAPI"
                };

                if (metadata != null)
                {
                    foreach (var kvp in metadata.Take(10)) // Stripe allows up to 20 metadata keys
                    {
                        var key = $"metadata[{kvp.Key}]";
                        if (!form.ContainsKey(key))
                            form[key] = kvp.Value ?? "";
                    }
                }

                var content = new FormUrlEncodedContent(form);
                var response = await _http.PostAsync("payment_intents", content);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var errCode = ExtractJson(body, "error.code") ?? "";
                    var errMsg  = ExtractJson(body, "error.message") ?? body;
                    return new PaymentIntentResult
                    {
                        Success = false,
                        ErrorCode = errCode,
                        ErrorMessage = $"Stripe API error {response.StatusCode}: {errMsg}"
                    };
                }

                var id       = ExtractJson(body, "id")         ?? "";
                var secret   = ExtractJson(body, "client_secret") ?? "";
                var status   = ExtractJson(body, "status")    ?? "";

                return new PaymentIntentResult
                {
                    Success = true,
                    TransactionId = id,
                    ClientSecret = secret,
                    GatewayTransactionId = id,
                    Metadata = new Dictionary<string, string>
                    {
                        ["stripe_status"] = status
                    }
                };
            }
            catch (HttpRequestException ex)
            {
                return new PaymentIntentResult
                {
                    Success = false,
                    ErrorCode = "NETWORK_ERROR",
                    ErrorMessage = "Could not reach Stripe API: " + ex.Message
                };
            }
            catch (TaskCanceledException ex)
            {
                return new PaymentIntentResult
                {
                    Success = false,
                    ErrorCode = "TIMEOUT",
                    ErrorMessage = "Stripe API request timed out: " + ex.Message
                };
            }
        }

        /// <inheritdoc />
        public Task<PaymentVerificationResult> VerifyWebhook(
            string rawBody,
            IDictionary<string, string> headers)
        {
            if (string.IsNullOrWhiteSpace(rawBody))
                return Task.FromResult(new PaymentVerificationResult
                {
                    SignatureValid = false,
                    ErrorMessage = "Empty webhook body"
                });

            // Extract Stripe signature header
            var sigHeader = headers?
                .FirstOrDefault(h => h.Key.Equals("Stripe-Signature", StringComparison.OrdinalIgnoreCase))
                .Value ?? "";

            if (!string.IsNullOrWhiteSpace(_webhookSecret) && !string.IsNullOrWhiteSpace(sigHeader))
            {
                // Verify using Stripe's HMAC-SHA256 scheme
                // Format: t=timestamp,v1=signature
                var isValid = VerifyStripeSignature(rawBody, sigHeader, _webhookSecret);
                if (!isValid)
                {
                    return Task.FromResult(new PaymentVerificationResult
                    {
                        SignatureValid = false,
                        RawPayload = Truncate(rawBody, 4000),
                        ErrorMessage = "Stripe webhook signature verification failed."
                    });
                }
            }

            // Parse the event
            var eventType = ExtractJson(rawBody, "type") ?? "";
            var eventId   = ExtractJson(rawBody, "id")   ?? "";
            var livemode  = ExtractJson(rawBody, "livemode") ?? "false";

            // Extract data.object (the PaymentIntent or charge object)
            var dataObject = ExtractJson(rawBody, "data.object") ?? "";
            var piId       = ExtractJson(dataObject, "id") ?? "";
            var status     = ExtractJson(dataObject, "status") ?? ExtractJson(dataObject, "amount") ?? "";
            var amountRaw  = ExtractJson(dataObject, "amount_received") ?? ExtractJson(dataObject, "amount") ?? "0";

            // Map Stripe status to our canonical status
            var canonicalStatus = MapStripeStatus(eventType, status);

            var result = new PaymentVerificationResult
            {
                SignatureValid = true,
                Gateway = "stripe",
                EventType = eventType,
                EventId = eventId,
                TransactionId = piId,
                RawPayload = Truncate(rawBody, 4000),
                Status = canonicalStatus,
                Metadata = new Dictionary<string, string>
                {
                    ["livemode"] = livemode,
                    ["amount_received"] = amountRaw
                }
            };

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public async Task<RefundResult> Refund(string transactionId, decimal amount)
        {
            if (!_enabled)
                return new RefundResult
                {
                    Success = false,
                    ErrorMessage = "Stripe gateway is disabled."
                };

            try
            {
                var form = new Dictionary<string, string>
                {
                    ["payment_intent"] = transactionId,
                    // amount in smallest unit; if amount == 0, full refund
                    ["confirm"] = "false"
                };

                if (amount > 0)
                    form["amount"] = ((long)amount).ToString();

                var content = new FormUrlEncodedContent(form);
                var response = await _http.PostAsync("refunds", content);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new RefundResult
                    {
                        Success = false,
                        ErrorCode = ExtractJson(body, "error.code") ?? "",
                        ErrorMessage = ExtractJson(body, "error.message") ?? body
                    };
                }

                return new RefundResult
                {
                    Success = true,
                    RefundId = ExtractJson(body, "id") ?? "",
                    Status = ExtractJson(body, "status") ?? "succeeded"
                };
            }
            catch (Exception ex)
            {
                return new RefundResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        // ─── Private helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Minimal JSON value extractor for flat paths like "data.object.status".
        /// Does NOT parse full JSON — safe for untrusted input without introducing
        /// a JSON deserialization dependency in this layer.
        /// </summary>
        private static string ExtractJson(string json, string path)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(path))
                return null;

            var segments = path.Split('.');
            var current  = json;

            foreach (var seg in segments)
            {
                current = ExtractObjectOrArray(current, seg);
                if (current == null) return null;
            }

            return ExtractValue(current);
        }

        private static string ExtractObjectOrArray(string json, string key)
        {
            // Look for "key": { or "key": [
            var search = $"\"{key}\"";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return null;

            var colon = json.IndexOf(':', idx);
            if (colon < 0) return null;

            var start = colon + 1;
            var trimmed = json.Substring(start).TrimStart();

            if (trimmed.StartsWith("{"))
            {
                var depth = 0;
                for (var i = 0; i < trimmed.Length; i++)
                {
                    if (trimmed[i] == '{' || trimmed[i] == '[') depth++;
                    else if (trimmed[i] == '}' || trimmed[i] == ']') { depth--; if (depth == 0) return trimmed.Substring(0, i + 1); }
                }
            }
            else if (trimmed.StartsWith("["))
            {
                var depth = 0;
                for (var i = 0; i < trimmed.Length; i++)
                {
                    if (trimmed[i] == '{' || trimmed[i] == '[') depth++;
                    else if (trimmed[i] == '}' || trimmed[i] == ']') { depth--; if (depth == 0) return trimmed.Substring(0, i + 1); }
                }
            }
            else
            {
                // Primitive value
                return trimmed;
            }
            return null;
        }

        private static string ExtractValue(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            json = json.TrimStart();
            if (json.StartsWith("\""))
            {
                // String value — find closing quote (handling escaped quotes)
                var end = 1;
                while (end < json.Length)
                {
                    if (json[end] == '"' && json[end - 1] != '\\') break;
                    end++;
                }
                return json.Substring(1, end - 1);
            }
            if (json.StartsWith("null"))  return null;
            if (json.StartsWith("true"))  return "true";
            if (json.StartsWith("false")) return "false";
            // Number
            var numEnd = 0;
            while (numEnd < json.Length && (char.IsDigit(json[numEnd]) || json[numEnd] == '.' || json[numEnd] == '-' || json[numEnd] == '+'))
                numEnd++;
            return numEnd > 0 ? json.Substring(0, numEnd) : null;
        }

        /// <summary>
        /// Stripe webhook signature verification per
        /// https://stripe.com/docs/webhooks/signatures
        /// </summary>
        private static bool VerifyStripeSignature(string payload, string header, string secret)
        {
            try
            {
                var parts = header.Split(',');
                string timestamp = null, sig = null;
                foreach (var part in parts)
                {
                    var kv = part.Split('=');
                    if (kv.Length == 2)
                    {
                        if (kv[0] == "t") timestamp = kv[1];
                        if (kv[0] == "v1") sig = kv[1];
                    }
                }

                if (string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(sig))
                    return false;

                // Reject events older than 5 minutes
                if (long.TryParse(timestamp, out var ts))
                {
                    var age = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ts;
                    if (age > 300 || age < -60) return false;
                }

                var signedPayload = $"{timestamp}.{payload}";
                using (var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret)))
                {
                    var computed = BitConverter.ToString(
                        hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload)))
                        .Replace("-", "").ToLowerInvariant();
                    return computed == sig.ToLowerInvariant();
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Maps Stripe event type + object status to our canonical donation status.
        /// </summary>
        private static string MapStripeStatus(string eventType, string objectStatus)
        {
            switch (eventType)
            {
                case "payment_intent.succeeded":
                case "charge.refunded":
                    return objectStatus == "succeeded" ? "succeeded" : "completed";
                case "payment_intent.payment_failed":
                case "charge.failed":
                    return "failed";
                default:
                    return objectStatus ?? "unknown";
            }
        }

        private static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s) || s.Length <= max) return s;
            return s.Substring(0, max);
        }
    }
}
