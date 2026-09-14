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
                var donation = _context.Donations
                    .Include(d => d.Cause)
                    .FirstOrDefault(d => d.DonationId == id && d.UserId == userId);

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

                // Validate amount
                if (request.Amount <= 0)
                {
                    return BadRequest("Amount must be greater than zero");
                }

                // Create donation
                var donation = new Donation
                {
                    UserId = userId,
                    CauseId = request.CauseId,
                    CampaignId = request.CampaignId,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = "Completed",
                    CardLastFour = request.CardLast4,
                    CardType = request.PaymentGateway ?? request.PaymentMethod,
                    TransactionId = GenerateTransactionId(),
                    Message = request.Message,
                    IsAnonymous = request.IsAnonymous,
                    ReceiptSent = false,
                    DonationDate = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                using (var transaction = _context.Database.BeginTransaction())
                {
                    _context.Donations.Add(donation);
                    _context.SaveChanges();

                    // Increment cached rollups in SQL so concurrent donations cannot overwrite each other.
                    var updatedAt = DateTime.Now;
                    _context.Database.ExecuteSqlCommand(
                        "UPDATE Causes SET raised_amount = COALESCE(raised_amount, 0) + @p0, updated_at = @p1 WHERE cause_id = @p2",
                        request.Amount, updatedAt, request.CauseId);

                    if (campaign != null)
                    {
                        _context.Database.ExecuteSqlCommand(
                            "UPDATE Campaigns SET raised_amount = COALESCE(raised_amount, 0) + @p0, updated_at = @p1 WHERE campaign_id = @p2",
                            request.Amount, updatedAt, campaign.CampaignId);
                    }

                    transaction.Commit();
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Donation successful! Thank you for your contribution.",
                    Data = new
                    {
                        donationId = donation.DonationId,
                        transactionId = donation.TransactionId,
                        amount = donation.Amount,
                        causeName = cause.CauseName,
                        campaignName = campaign?.CampaignName,
                        donationDate = donation.DonationDate
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
        [Authorize(Roles = "SuperAdmin,Admin")]
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
    }
}

