using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using GiveAID.Web.Data;
using GiveAID.Web.Models;
using GiveAID.Web.Helpers;
using GiveAID.Web.Controllers;
using GiveAID.Web.Services.Payments;

namespace GiveAID.Web.Controllers
{
    [RoutePrefix("api/donations")]
    [JwtAuthorize]
    public class DonationsController : ApiController
    {
        private readonly GiveAIDContext _context;

        public DonationsController()
        {
            _context = new GiveAIDContext();
        }

        // GET: api/donations
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromToken(Request);

                var query = _context.Donations
                    .Include(d => d.Cause)
                    .Include(d => d.Campaign)
                    .Where(d => d.UserId == userId)
                    .OrderByDescending(d => d.DonationDate);

                var total = query.Count();
                var donations = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        items = donations.Select(d => new
                        {
                            donationId = d.DonationId,
                            causeId = d.CauseId,
                            causeName = d.Cause.CauseName,
                            campaignId = d.CampaignId,
                            campaignName = d.Campaign != null ? d.Campaign.CampaignName : (string)null,
                            amount = d.Amount,
                            paymentMethod = d.PaymentMethod,
                            paymentStatus = d.PaymentStatus,
                            transactionId = d.TransactionId,
                            message = d.Message,
                            isAnonymous = d.IsAnonymous,
                            donationDate = d.DonationDate
                        }).ToList(),
                        total,
                        page,
                        pageSize,
                        totalPages = (int)Math.Ceiling((double)total / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/donations/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromToken(Request);

                // IDOR defence: resolve the caller's role first. If the caller
                // is NOT admin, scope the lookup to their own donations. This
                // means a non-admin asking for someone else's donation id gets
                // a clean 404 (looks like "doesn't exist" instead of "exists but
                // forbidden"), which prevents enumeration of other users'
                // donation ids by status code.
                var caller = _context.Users.Find(userId);
                var isAdmin = caller != null && (caller.Role == "Admin" || caller.Role == "SuperAdmin");

                Donation donation;
                if (isAdmin)
                {
                    donation = _context.Donations
                        .Include(d => d.Cause)
                        .Include(d => d.Campaign)
                        .FirstOrDefault(d => d.DonationId == id);
                }
                else
                {
                    donation = _context.Donations
                        .Include(d => d.Cause)
                        .Include(d => d.Campaign)
                        .FirstOrDefault(d => d.DonationId == id && d.UserId == userId);
                }

                // SECURITY: return 404 for both "doesn't exist" AND
                // "exists but not yours". This way attackers can't probe
                // donation ids to learn which are valid.
                if (donation == null)
                {
                    return NotFound();
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        donationId = donation.DonationId,
                        userId = donation.UserId,
                        causeId = donation.CauseId,
                        causeName = donation.Cause.CauseName,
                        campaignId = donation.CampaignId,
                        campaignName = donation.Campaign != null ? donation.Campaign.CampaignName : null,
                        amount = donation.Amount,
                        paymentMethod = donation.PaymentMethod,
                        paymentStatus = donation.PaymentStatus,
                        cardLastFour = donation.CardLastFour,
                        transactionId = donation.TransactionId,
                        message = donation.Message,
                        isAnonymous = donation.IsAnonymous,
                        receiptSent = donation.ReceiptSent,
                        donationDate = donation.DonationDate
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/donations
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Create(DonationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = JwtHelper.GetUserIdFromToken(Request);

                // Validate amount
                if (request.Amount <= 0)
                {
                    return BadRequest("Amount must be greater than zero");
                }

                // SECURITY/PERF: Idempotency check — if the client supplied an
                // IdempotencyKey and a donation already exists for this user with
                // the same key, return the existing donation instead of creating a
                // duplicate. Prevents double-tap and network-retry duplicates.
                Donation existingDonation = null;
                if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
                {
                    existingDonation = _context.Donations
                        .Include(d => d.Cause)
                        .Include(d => d.Campaign)
                        .FirstOrDefault(d =>
                            d.UserId == userId &&
                            d.IdempotencyKey == request.IdempotencyKey);
                    if (existingDonation != null)
                    {
                        return Ok(new ApiResponse
                        {
                            Success = true,
                            Message = "Donation already recorded (idempotent).",
                            Data = new
                            {
                                donationId = existingDonation.DonationId,
                                causeId = existingDonation.CauseId,
                                causeName = existingDonation.Cause?.CauseName,
                                campaignId = existingDonation.CampaignId,
                                campaignName = existingDonation.Campaign != null ? existingDonation.Campaign.CampaignName : null,
                                amount = existingDonation.Amount,
                                paymentMethod = existingDonation.PaymentMethod,
                                paymentStatus = existingDonation.PaymentStatus,
                                transactionId = existingDonation.TransactionId,
                                message = existingDonation.Message,
                                isAnonymous = existingDonation.IsAnonymous,
                                donationDate = existingDonation.DonationDate,
                                isIdempotent = true
                            }
                        });
                    }
                }

                // Validate cause exists
                var cause = _context.Causes.Find(request.CauseId);
                if (cause == null || !cause.IsActive)
                {
                    return BadRequest("Invalid or inactive cause");
                }

                // Validate campaign if provided
                Campaign campaign = null;
                if (request.CampaignId.HasValue)
                {
                    campaign = _context.Campaigns.Find(request.CampaignId.Value);
                    if (campaign == null)
                    {
                        return BadRequest("Invalid campaign");
                    }
                    if (campaign.CauseId != request.CauseId)
                    {
                        return BadRequest("Campaign does not belong to the selected cause");
                    }
                    if (campaign.Status != "Active" || campaign.StartDate > DateTime.Today ||
                        (campaign.EndDate.HasValue && campaign.EndDate.Value < DateTime.Today))
                    {
                        return BadRequest("Campaign is not active");
                    }
                }

                // Determine the payment gateway to use.
                // Default to "mock" if not specified. Real deployments set
                // PaymentGateway__Default in Web.config to "stripe" (or "vnpay"/"momo").
                var gatewayName = string.IsNullOrWhiteSpace(request.PaymentGateway)
                    ? PaymentGatewayFactory.CurrentGatewayName
                    : request.PaymentGateway.Trim().ToLowerInvariant();

                // ── Call the payment gateway to create a PaymentIntent ─────────────
                var gateway = PaymentGatewayFactory.Current;
                var metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["donation_user_id"]  = userId.ToString(),
                    ["donation_cause_id"] = request.CauseId.ToString(),
                    ["donation_campaign_id"] = request.CampaignId?.ToString() ?? "",
                    ["donation_gateway"] = gatewayName
                };

                var intentResult = await gateway.CreatePaymentIntent(
                    amount:      request.Amount,
                    currency:    "vnd",
                    description: $"GiveAID donation to {cause.CauseName}" +
                                 (campaign != null ? $" (Campaign: {campaign.CampaignName})" : ""),
                    metadata:    metadata);

                if (!intentResult.Success)
                {
                    return Content(System.Net.HttpStatusCode.BadGateway, new ApiResponse
                    {
                        Success = false,
                        Message = "Payment gateway error: " + (intentResult.ErrorMessage ?? intentResult.ErrorCode ?? "Unknown error")
                    });
                }

                // ── Create donation record in PENDING state ────────────────────────
                var donation = new Donation
                {
                    UserId = userId,
                    CauseId = request.CauseId,
                    CampaignId = request.CampaignId,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = "Pending",
                    CardLastFour = request.CardLast4,
                    CardType = request.PaymentGateway ?? request.PaymentMethod,
                    TransactionId = GenerateTransactionId(),
                    GatewayTransactionId = intentResult.GatewayTransactionId,
                    ClientSecret = intentResult.ClientSecret,
                    PaymentGateway = gatewayName,
                    Message = request.Message?.Trim(),
                    IsAnonymous = request.IsAnonymous,
                    ReceiptSent = false,
                    DonationDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    IdempotencyKey = request.IdempotencyKey
                };

                try
                {
                    _context.Donations.Add(donation);
                    _context.SaveChanges();
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException)
                {
                    // Race condition: another request with the same IdempotencyKey
                    // beat us. Re-fetch and return the existing donation.
                    _context.Entry(donation).State = System.Data.Entity.EntityState.Detached;
                    var raceWinner = _context.Donations
                        .Include(d => d.Cause)
                        .FirstOrDefault(d =>
                            d.UserId == userId &&
                            d.IdempotencyKey == request.IdempotencyKey);
                    if (raceWinner != null)
                    {
                        return Ok(new ApiResponse
                        {
                            Success = true,
                            Message = "Donation already recorded (idempotent).",
                            Data = new
                            {
                                donationId = raceWinner.DonationId,
                                transactionId = raceWinner.TransactionId,
                                amount = raceWinner.Amount,
                                causeName = raceWinner.Cause?.CauseName,
                                donationDate = raceWinner.DonationDate,
                                paymentStatus = raceWinner.PaymentStatus,
                                isIdempotent = true
                            }
                        });
                    }
                    throw;
                }

                // NOTE: We intentionally do NOT increment cause.cached raised_amount
                // / campaign.raised_amount until payment is confirmed. That keeps
                // the dashboard KPIs accurate to actual money received, not
                // pending intentions.

                // Return donation details plus the clientSecret so the frontend
                // can complete the payment via Stripe.js (or equivalent SDK).
                var stripePublishableKey = PaymentGatewayFactory.StripePublishableKey;

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Donation intent created. Complete payment using the clientSecret.",
                    Data = new
                    {
                        donationId = donation.DonationId,
                        transactionId = donation.TransactionId,
                        amount = donation.Amount,
                        causeName = cause.CauseName,
                        campaignName = campaign?.CampaignName,
                        donationDate = donation.DonationDate,
                        paymentStatus = donation.PaymentStatus,
                        isIdempotent = false,
                        // Gateway-specific fields
                        clientSecret = donation.ClientSecret,
                        gatewayTransactionId = donation.GatewayTransactionId,
                        paymentGateway = gatewayName,
                        // Frontend needs the publishable key to initialise Stripe.js
                        stripePublishableKey = stripePublishableKey ?? (object)null
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/donations/stats
        [HttpGet]
        [Route("stats")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult GetStats()
        {
            try
            {
                var completedDonations = _context.Donations
                    .Where(d => d.PaymentStatus == "Completed");
                var totalDonations = completedDonations.Count();
                var totalAmount = completedDonations.Sum(d => (decimal?)d.Amount) ?? 0;
                var averageDonation = totalDonations > 0 ? totalAmount / totalDonations : 0;
                var uniqueDonors = completedDonations.Select(d => d.UserId).Distinct().Count();

                var recentDonations = completedDonations
                    .Include(d => d.User)
                    .Include(d => d.Cause)
                    .OrderByDescending(d => d.DonationDate)
                    .Take(10)
                    .Select(d => new
                    {
                        donationId = d.DonationId,
                        donorName = d.IsAnonymous ? "Anonymous" : d.User.FullName,
                        causeName = d.Cause.CauseName,
                        amount = d.Amount,
                        donationDate = d.DonationDate
                    })
                    .ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        totalDonations,
                        totalAmount,
                        averageDonation,
                        uniqueDonors,
                        recentDonations
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/donations/{id}/confirm
        // Admin-only endpoint to manually mark a Pending donation as Completed.
        // In production this would be replaced by an authenticated payment-gateway
        // webhook that verifies the charge via the gateway API before flipping
        // status. For now admins can confirm after verifying in the gateway
        // dashboard. Idempotent: re-confirming a Completed donation is a no-op.
        [HttpPost]
        [Route("{id:int}/confirm")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult ConfirmPayment(int id, [FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                var donation = _context.Donations
                    .Include(d => d.Cause)
                    .Include(d => d.Campaign)
                    .FirstOrDefault(d => d.DonationId == id);
                if (donation == null) return NotFound();

                if (donation.PaymentStatus == "Completed")
                {
                    return Ok(new ApiResponse
                    {
                        Success = true,
                        Message = "Donation was already confirmed.",
                        Data = new { donation.DonationId, donation.PaymentStatus, donation.PaymentConfirmedAt }
                    });
                }
                if (donation.PaymentStatus == "Failed" || donation.PaymentStatus == "Refunded")
                {
                    return BadRequest("Cannot confirm a donation that is " + donation.PaymentStatus + ".");
                }

                donation.PaymentStatus = "Completed";
                donation.PaymentConfirmedAt = DateTime.UtcNow;
                donation.GatewayTransactionId = request?.GatewayTransactionId ?? donation.TransactionId;

                // Roll up the now-confirmed amount into the cause / campaign totals.
                // Use raw SQL so concurrent donations do not lose updates.
                var updatedAt = DateTime.Now;
                _context.Database.ExecuteSqlCommand(
                    "UPDATE Causes SET raised_amount = COALESCE(raised_amount, 0) + @p0, updated_at = @p1 WHERE cause_id = @p2",
                    donation.Amount, updatedAt, donation.CauseId);
                if (donation.CampaignId.HasValue)
                {
                    _context.Database.ExecuteSqlCommand(
                        "UPDATE Campaigns SET raised_amount = COALESCE(raised_amount, 0) + @p0, updated_at = @p1 WHERE campaign_id = @p2",
                        donation.Amount, updatedAt, donation.CampaignId.Value);
                }

                _context.SaveChanges();

                // Invalidate statistics cache after donation confirmation
                CacheHelper.InvalidateStatistics();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Donation confirmed. Cached totals updated.",
                    Data = new
                    {
                        donation.DonationId,
                        donation.PaymentStatus,
                        donation.PaymentConfirmedAt,
                        donation.GatewayTransactionId
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/donations/webhook
        // Public endpoint for payment-gateway webhooks (Stripe, VNPay, MoMo, Mock).
        // No JWT required — the gateway authenticates via its own signature header.
        // IMPORTANT: Return 200 quickly (< 2 seconds) so the gateway doesn't retry.
        // All signature verification and business logic happens before the response.
        [HttpPost]
        [Route("webhook")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> PaymentWebhook()
        {
            // Read raw body — required for signature verification (Stripe HMAC-SHA256)
            string rawBody;
            try
            {
                rawBody = await Request.Content.ReadAsStringAsync();
            }
            catch
            {
                rawBody = string.Empty;
            }
            if (string.IsNullOrWhiteSpace(rawBody))
                return BadRequest("Empty webhook body.");

            // Collect headers for signature verification
            var headers = Request.Headers
                .ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

            // ── 1. Verify the webhook using the active gateway ───────────────────
            var gateway = PaymentGatewayFactory.Current;
            var verification = await gateway.VerifyWebhook(rawBody, headers);

            // ── 2. Log every webhook event before processing ──────────────────────
            var sigHeader = headers
                .FirstOrDefault(h => h.Key.Equals("Stripe-Signature", StringComparison.OrdinalIgnoreCase))
                .Value ?? "";

            var logEntry = new WebhookLog
            {
                Gateway = gateway.Name,
                EventType = verification.EventType,
                EventId = verification.EventId,
                RawPayload = verification.RawPayload?.Length > 4000
                    ? verification.RawPayload.Substring(0, 4000)
                    : (verification.RawPayload ?? ""),
                Signature = sigHeader,
                SignatureValid = verification.SignatureValid,
                ProcessingStatus = "Processed",
                ReceivedAt = DateTime.UtcNow
            };

            // Idempotency: if this EventId was already logged, return 200 immediately.
            if (!string.IsNullOrWhiteSpace(verification.EventId))
            {
                var duplicate = _context.WebhookLogs
                    .FirstOrDefault(w => w.EventId == verification.EventId && w.Gateway == gateway.Name);
                if (duplicate != null)
                {
                    logEntry.ProcessingStatus = "Duplicate";
                    _context.WebhookLogs.Add(logEntry);
                    _context.SaveChanges();
                    return Ok(new { success = true, duplicate = true });
                }
            }

            // ── 3. Look up the donation ───────────────────────────────────────────
            Donation donation = null;
            if (!string.IsNullOrWhiteSpace(verification.TransactionId))
            {
                donation = _context.Donations
                    .FirstOrDefault(d =>
                        d.GatewayTransactionId == verification.TransactionId ||
                        d.TransactionId == verification.TransactionId);
            }

            if (donation != null)
            {
                logEntry.DonationId = donation.DonationId;
                logEntry.DonationTransactionId = donation.TransactionId;
            }

            // ── 4. Apply status change ───────────────────────────────────────────
            if (donation != null)
            {
                try
                {
                    await ApplyWebhookStatusChange(donation, verification);
                    logEntry.ProcessingStatus = "Processed";
                }
                catch (Exception ex)
                {
                    logEntry.ProcessingStatus = "Failed";
                    logEntry.ErrorMessage = ex.Message;
                    System.Diagnostics.Debug.WriteLine($"[Webhook] Error processing event {verification.EventId}: {ex}");
                }
            }
            else
            {
                logEntry.ProcessingStatus = "Ignored";
                logEntry.ErrorMessage = "No matching donation found for transaction: " + verification.TransactionId;
            }

            logEntry.ProcessedAt = DateTime.UtcNow;
            _context.WebhookLogs.Add(logEntry);
            _context.SaveChanges();

            // Always return 200 to acknowledge receipt (prevents gateway retries)
            return Ok(new { success = true });
        }

        private Task ApplyWebhookStatusChange(Donation donation, PaymentVerificationResult result)
        {
            if (donation == null || result == null) return Task.CompletedTask;

            var now = DateTime.UtcNow;

            switch ((result.Status ?? "").ToLowerInvariant())
            {
                case "succeeded":
                case "completed":
                case "paid":
                    if (donation.PaymentStatus != "Completed")
                    {
                        donation.PaymentStatus = "Completed";
                        donation.PaymentConfirmedAt = now;
                        donation.GatewayTransactionId = result.TransactionId ?? donation.GatewayTransactionId;

                        var updatedAt = DateTime.Now;
                        _context.Database.ExecuteSqlCommand(
                            "UPDATE Causes SET raised_amount = COALESCE(raised_amount, 0) + @p0, updated_at = @p1 WHERE cause_id = @p2",
                            donation.Amount, updatedAt, donation.CauseId);
                        if (donation.CampaignId.HasValue)
                        {
                            _context.Database.ExecuteSqlCommand(
                                "UPDATE Campaigns SET raised_amount = COALESCE(raised_amount, 0) + @p0, updated_at = @p1 WHERE campaign_id = @p2",
                                donation.Amount, updatedAt, donation.CampaignId.Value);
                        }

                        // Invalidate statistics cache after successful payment
                        CacheHelper.InvalidateStatistics();
                    }
                    break;

                case "failed":
                case "declined":
                    donation.PaymentStatus = "Failed";
                    break;

                case "refunded":
                    donation.PaymentStatus = "Refunded";
                    _context.Database.ExecuteSqlCommand(
                        "UPDATE Causes SET raised_amount = COALESCE(raised_amount, 0) - @p0, updated_at = GETDATE() WHERE cause_id = @p1",
                        donation.Amount, donation.CauseId);
                    if (donation.CampaignId.HasValue)
                    {
                        _context.Database.ExecuteSqlCommand(
                            "UPDATE Campaigns SET raised_amount = COALESCE(raised_amount, 0) - @p0, updated_at = GETDATE() WHERE campaign_id = @p1",
                            donation.Amount, donation.CampaignId.Value);
                    }
                    break;

                default:
                    // Unknown status — log it but don't crash
                    System.Diagnostics.Debug.WriteLine(
                        $"[Webhook] Unknown status '{result.Status}' for donation {donation.DonationId} " +
                        $"(event: {result.EventType}, txn: {result.TransactionId})");
                    break;
            }

            return Task.CompletedTask;
        }

        private string GenerateTransactionId()
        {
            return $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid():N}";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class ConfirmPaymentRequest
    {
        public string GatewayTransactionId { get; set; }
    }

    public class GatewayWebhookPayload
    {
        public string TransactionId { get; set; }
        public string Status { get; set; } // "succeeded" | "failed" | "refunded"
        public string Signature { get; set; }
    }

    public class DonationRequest
    {
        public int CauseId { get; set; }
        public int? CampaignId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }

        // SECURITY: PCI-DSS compliance â€” payment card data (PAN, CVV, expiry)
        // MUST be tokenized by a PCI-compliant payment gateway (Stripe, Braintree,
        // VNPay, MoMo, etc.) BEFORE reaching this endpoint. We accept only the
        // opaque payment-token returned by the gateway; raw card numbers and
        // CVV are never accepted, logged, or stored.
        public string PaymentToken { get; set; }
        public string PaymentGateway { get; set; } // e.g. "stripe", "vnpay", "momo"

        // Last 4 digits of the card for display purposes only (provided by gateway).
        public string CardLast4 { get; set; }

        [MaxLength(500)]
        public string Message { get; set; }
        public bool IsAnonymous { get; set; }

        // Optional client-generated idempotency key. If supplied and a donation
        // already exists with the same TransactionId for this user, the existing
        // donation is returned instead of creating a duplicate. Recommended for
        // mobile clients to survive retries / double-tap.
        [MaxLength(100)]
        public string IdempotencyKey { get; set; }
    }
}

