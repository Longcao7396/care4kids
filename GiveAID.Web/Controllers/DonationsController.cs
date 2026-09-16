using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http;
using System.Data.Entity;
using GiveAID.Web.Data;
using GiveAID.Web.Models;
using GiveAID.Web.Helpers;
using GiveAID.Web.Controllers;

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
                            amount = d.Amount,
                            paymentMethod = d.PaymentMethod,
                            paymentStatus = d.PaymentStatus,
                            transactionId = d.TransactionId,
                            campaignId = d.CampaignId,
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
        public IHttpActionResult Create(DonationRequest request)
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
                                transactionId = existingDonation.TransactionId,
                                amount = existingDonation.Amount,
                                causeName = existingDonation.Cause?.CauseName,
                                donationDate = existingDonation.DonationDate,
                                paymentStatus = existingDonation.PaymentStatus,
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

                // Create donation in PENDING state. The old behaviour of marking
                // "Completed" immediately was a financial-reporting bug: no payment
                // gateway was actually called. Now the gateway webhook (or admin
                // manual confirmation) is responsible for flipping status to
                // "Completed". This prevents inflated donation totals.
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

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Donation recorded. It will be marked Completed once payment is confirmed by the gateway.",
                    Data = new
                    {
                        donationId = donation.DonationId,
                        transactionId = donation.TransactionId,
                        amount = donation.Amount,
                        causeName = cause.CauseName,
                        campaignName = campaign?.CampaignName,
                        donationDate = donation.DonationDate,
                        paymentStatus = donation.PaymentStatus,
                        isIdempotent = false
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
        // Public endpoint intended for payment-gateway webhooks (Stripe, VNPay, MoMo).
        // No JWT required — the gateway authenticates via its own signature header
        // (X-Gateway-Signature). For now this endpoint accepts any POST with a
        // matching transactionId; real-world deployments must verify the signature
        // against the gateway's shared secret before trusting the payload.
        //
        // In production this endpoint should:
        //   1. Verify the gateway signature
        //   2. Look up the donation by gateway_transaction_id (or our internal id)
        //   3. Flip status to Completed (or Failed for disputes/chargebacks)
        //   4. Return 200 quickly so the gateway doesn't retry
        [HttpPost]
        [Route("webhook")]
        [AllowAnonymous]
        public IHttpActionResult PaymentWebhook([FromBody] GatewayWebhookPayload payload)
        {
            try
            {
                if (payload == null || string.IsNullOrWhiteSpace(payload.TransactionId))
                    return BadRequest("transactionId is required.");

                // TODO: verify X-Gateway-Signature header against the gateway's shared secret.

                var donation = _context.Donations
                    .FirstOrDefault(d =>
                        d.TransactionId == payload.TransactionId ||
                        d.GatewayTransactionId == payload.TransactionId);
                if (donation == null) return NotFound();

                switch ((payload.Status ?? "").ToLowerInvariant())
                {
                    case "succeeded":
                    case "completed":
                    case "paid":
                        if (donation.PaymentStatus != "Completed")
                        {
                            donation.PaymentStatus = "Completed";
                            donation.PaymentConfirmedAt = DateTime.UtcNow;
                            donation.GatewayTransactionId = payload.TransactionId;

                            _context.Database.ExecuteSqlCommand(
                                "UPDATE Causes SET raised_amount = COALESCE(raised_amount, 0) + @p0, updated_at = @p1 WHERE cause_id = @p2",
                                donation.Amount, DateTime.Now, donation.CauseId);
                            if (donation.CampaignId.HasValue)
                            {
                                _context.Database.ExecuteSqlCommand(
                                    "UPDATE Campaigns SET raised_amount = COALESCE(raised_amount, 0) + @p0, updated_at = @p1 WHERE campaign_id = @p2",
                                    donation.Amount, DateTime.Now, donation.CampaignId.Value);
                            }
                        }
                        break;
                    case "failed":
                    case "declined":
                        donation.PaymentStatus = "Failed";
                        break;
                    case "refunded":
                        donation.PaymentStatus = "Refunded";
                        break;
                    default:
                        return BadRequest("Unknown status: " + payload.Status);
                }

                _context.SaveChanges();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
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

