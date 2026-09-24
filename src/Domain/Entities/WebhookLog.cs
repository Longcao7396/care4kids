using System;

namespace GiveAID.Domain.Entities
{
    /// <summary>
    /// Persistent log of every webhook event received from payment gateways.
    /// Useful for debugging gateway delivery issues, auditing events, and
    /// detecting duplicate deliveries (idempotency checks).
    /// </summary>
    public class WebhookLog
    {
        /// <summary>
        /// Uses long (int64) as primary key — high-volume table.
        /// </summary>
        public long WebhookLogId { get; set; }

        /// <summary>
        /// Gateway that sent this event: "stripe", "vnpay", "momo", "mock".
        /// </summary>
        public string Gateway { get; set; } = string.Empty;

        /// <summary>
        /// Event type from the gateway, e.g. "payment_intent.succeeded".
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Unique event ID from the gateway (for idempotency / deduplication).
        /// </summary>
        public string EventId { get; set; } = string.Empty;

        /// <summary>
        /// Raw webhook payload (truncated to 4000 chars).
        /// Stored so events can be replayed or audited.
        /// </summary>
        public string RawPayload { get; set; } = string.Empty;

        /// <summary>
        /// Signature header value received (for debugging failures).
        /// </summary>
        public string Signature { get; set; } = string.Empty;

        /// <summary>
        /// Whether signature verification passed.
        /// False = event was processed with a warning but status is untrusted.
        /// </summary>
        public bool SignatureValid { get; set; }

        /// <summary>
        /// Processed | Failed | Ignored | Duplicate
        /// </summary>
        public string ProcessingStatus { get; set; } = "Processed";

        /// <summary>
        /// Human-readable error message when ProcessingStatus = Failed.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// The donation's internal TransactionId if one was matched.
        /// </summary>
        public string DonationTransactionId { get; set; } = string.Empty;

        /// <summary>
        /// The donation's internal DonationId if one was matched.
        /// </summary>
        public int? DonationId { get; set; }

        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }
    }
}
