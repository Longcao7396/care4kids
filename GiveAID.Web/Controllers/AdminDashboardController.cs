using System;
using System.Linq;
using System.Web.Http;
using System.Data.Entity;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;

namespace GiveAID.Web.Controllers
{
    [RoutePrefix("api/admin")]
    public class AdminDashboardController : ApiController
    {
        private GiveAIDContext db = new GiveAIDContext();

        // GET: api/admin/stats
        [HttpGet]
        [Route("stats")]
        public IHttpActionResult GetDashboardStats()
        {
            try
            {
                var auth = JwtHelper.CheckAdmin(Request);
                if (!auth) return Unauthorized();

                // Total Donations
                var totalDonations = db.Donations
                    .Where(d => d.PaymentStatus == "Completed")
                    .Sum(d => (decimal?)d.Amount) ?? 0;

                var totalDonors = db.Donations
                    .Where(d => d.PaymentStatus == "Completed")
                    .Select(d => d.UserId)
                    .Distinct()
                    .Count();

                // Campaigns Stats
                var activeCampaigns = db.Campaigns.Count(c => c.Status == "Active");
                var completedCampaigns = db.Campaigns.Count(c => c.Status == "Completed");

                // Active Programmes
                var activeProgrammes = db.Programmes.Count(p => p.Status == "Ongoing" || p.Status == "Upcoming");

                // Recent Donations (Last 30 days)
                var thirtyDaysAgo = DateTime.Now.AddDays(-30);
                var recentDonationsAmount = db.Donations
                    .Where(d => d.PaymentStatus == "Completed" && d.DonationDate >= thirtyDaysAgo)
                    .Sum(d => (decimal?)d.Amount) ?? 0;

                // Donations by Month (Last 6 months)
                var sixMonthsAgo = DateTime.Now.AddMonths(-6);
                var donationsByMonth = db.Donations
                    .Where(d => d.PaymentStatus == "Completed" && d.DonationDate >= sixMonthsAgo)
                    .GroupBy(d => new { 
                        Year = d.DonationDate.Year, 
                        Month = d.DonationDate.Month 
                    })
                    .Select(g => new
                    {
                        year = g.Key.Year,
                        month = g.Key.Month,
                        total = g.Sum(d => d.Amount),
                        count = g.Count()
                    })
                    .OrderBy(x => x.year)
                    .ThenBy(x => x.month)
                    .ToList();

                // Donations by Campaign (Top 5)
                // Pre-compute donor counts in a single query to avoid N+1 subqueries
                var donorCountsByCampaign = db.Donations
                    .Where(d => d.PaymentStatus == "Completed" && d.CampaignId != null)
                    .GroupBy(d => d.CampaignId)
                    .Select(g => new { CampaignId = g.Key, donorCount = g.Select(d => d.UserId).Distinct().Count() })
                    .ToDictionary(x => x.CampaignId, x => x.donorCount);

                var donationsByCampaign = db.Campaigns
                    .Include(c => c.Cause)
                    .Where(c => c.Status == "Active")
                    .Select(c => new
                    {
                        campaignId = c.CampaignId,
                        campaignName = c.CampaignName,
                        causeName = c.Cause.CauseName,
                        goalAmount = c.GoalAmount,
                        raisedAmount = c.RaisedAmount,
                        // Cast to (double?) to force SQL floating-point division —
                        // otherwise (decimal / decimal) returns integer truncation
                        // when goal < raised, showing "0%" for partially-funded
                        // campaigns. We round to 0.1 precision on the client.
                        percentageReached = c.GoalAmount > 0
                            ? Math.Round((double)((c.RaisedAmount / c.GoalAmount) * 100m), 1)
                            : 0,
                        donorCount = 0
                    })
                    .ToList()
                    .Select(c => new {
                        c.campaignId,
                        c.campaignName,
                        c.causeName,
                        c.goalAmount,
                        c.raisedAmount,
                        c.percentageReached,
                        donorCount = donorCountsByCampaign.ContainsKey(c.campaignId) ? donorCountsByCampaign[c.campaignId] : 0
                    })
                    .OrderByDescending(x => x.raisedAmount)
                    .Take(5)
                    .ToList();

                // Donations by Cause
                // Pre-compute donation counts in a single query to avoid N+1 subqueries
                var donationCountsByCause = db.Donations
                    .Where(d => d.PaymentStatus == "Completed" && d.CauseId != null)
                    .GroupBy(d => d.CauseId)
                    .Select(g => new { CauseId = g.Key, donationCount = g.Count() })
                    .ToDictionary(x => x.CauseId, x => x.donationCount);

                var donationsByCause = db.Causes
                    .Select(c => new
                    {
                        causeId = c.CauseId,
                        causeName = c.CauseName,
                        causeCode = c.CauseCode,
                        targetAmount = c.TargetAmount,
                        raisedAmount = c.RaisedAmount,
                        donationCount = 0
                    })
                    .ToList()
                    .Select(c => new {
                        c.causeId,
                        c.causeName,
                        c.causeCode,
                        c.targetAmount,
                        c.raisedAmount,
                        donationCount = donationCountsByCause.ContainsKey(c.causeId) ? donationCountsByCause[c.causeId] : 0
                    })
                    .OrderByDescending(x => x.raisedAmount)
                    .ToList();

                // Recent Users (Last 10)
                var recentUsers = db.Users
                    .Where(u => u.Role == "User")
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(10)
                    .Select(u => new
                    {
                        u.UserId,
                        u.FullName,
                        u.Email,
                        u.CreatedAt
                    })
                    .ToList();

                // Programme Registrations Count
                var programmeRegistrations = db.ProgrammeRegistrations.Count();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        overview = new
                        {
                            totalDonations,
                            totalDonors,
                            activeCampaigns,
                            completedCampaigns,
                            activeProgrammes,
                            recentDonationsAmount,
                            programmeRegistrations
                        },
                        donationsByMonth,
                        donationsByCampaign,
                        donationsByCause,
                        recentUsers
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/admin/recent-donations
        [HttpGet]
        [Route("recent-donations")]
        public IHttpActionResult GetRecentDonations(int count = 20)
        {
            try
            {
                var auth = JwtHelper.CheckAdmin(Request);
                if (!auth) return Unauthorized();

                var donations = db.Donations
                    .Include(d => d.User)
                    .Include(d => d.Cause)
                    .Include(d => d.Campaign)
                    .Where(d => d.PaymentStatus == "Completed")
                    .OrderByDescending(d => d.DonationDate)
                    .Take(count)
                    .Select(d => new
                    {
                        d.DonationId,
                        d.Amount,
                        d.DonationDate,
                        d.TransactionId,
                        d.IsAnonymous,
                        userName = d.IsAnonymous ? "Anonymous" : d.User.FullName,
                        causeName = d.Cause.CauseName,
                        campaignName = d.Campaign != null ? d.Campaign.CampaignName : null
                    })
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = donations
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/admin/users-stats
        [HttpGet]
        [Route("users-stats")]
        public IHttpActionResult GetUsersStats()
        {
            try
            {
                var auth = JwtHelper.CheckAdmin(Request);
                if (!auth) return Unauthorized();

                var totalUsers = db.Users.Count(u => u.Role == "User");
                var activeUsers = db.Users.Count(u => u.Role == "User" && u.IsActive);
                var newUsersThisMonth = db.Users.Count(u =>
                    u.Role == "User" &&
                    u.CreatedAt.Year == DateTime.Now.Year &&
                    u.CreatedAt.Month == DateTime.Now.Month);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        totalUsers,
                        activeUsers,
                        newUsersThisMonth
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/admin/donations
        [HttpGet]
        [Route("donations")]
        public IHttpActionResult GetAllDonations(
            int page = 1,
            int pageSize = 20,
            string status = null,
            string search = null,
            int? campaignId = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            try
            {
                var auth = JwtHelper.CheckAdmin(Request);
                if (!auth) return Unauthorized();

                var query = db.Donations
                    .Include(d => d.User)
                    .Include(d => d.Cause)
                    .Include(d => d.Campaign)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(d => d.PaymentStatus == status);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();
                    query = query.Where(d =>
                        d.TransactionId.ToLower().Contains(s) ||
                        d.User.FullName.ToLower().Contains(s) ||
                        d.User.Email.ToLower().Contains(s) ||
                        d.Cause.CauseName.ToLower().Contains(s));
                }

                if (campaignId.HasValue)
                {
                    query = query.Where(d => d.CampaignId == campaignId.Value);
                }

                if (dateFrom.HasValue)
                {
                    query = query.Where(d => d.DonationDate >= dateFrom.Value);
                }

                if (dateTo.HasValue)
                {
                    query = query.Where(d => d.DonationDate <= dateTo.Value);
                }

                var total = query.Count();
                var items = query
                    .OrderByDescending(d => d.DonationDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(d => new
                    {
                        donationId = d.DonationId,
                        amount = d.Amount,
                        paymentMethod = d.PaymentMethod,
                        paymentStatus = d.PaymentStatus,
                        transactionId = d.TransactionId,
                        isAnonymous = d.IsAnonymous,
                        donationDate = d.DonationDate,
                        userId = d.UserId,
                        userName = d.IsAnonymous ? "Anonymous" : d.User.FullName,
                        userEmail = d.IsAnonymous ? null : d.User.Email,
                        causeId = d.CauseId,
                        causeName = d.Cause.CauseName,
                        campaignId = d.CampaignId,
                        campaignName = d.Campaign != null ? d.Campaign.CampaignName : null,
                        message = d.Message
                    })
                    .ToList();

                var totalAmount = query.Sum(d => (decimal?)d.Amount) ?? 0;
                var averageAmount = total > 0 ? totalAmount / total : 0;

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        items,
                        total,
                        page,
                        pageSize,
                        totalPages = (int)Math.Ceiling((double)total / pageSize),
                        summary = new
                        {
                            totalAmount,
                            averageAmount
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}


