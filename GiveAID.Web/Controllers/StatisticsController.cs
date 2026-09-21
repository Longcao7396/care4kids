using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;

namespace GiveAID.Web.Controllers
{
    /// <summary>
    /// Provides aggregated statistics from the database for the dashboard and homepage.
    /// Caches results in memory to avoid hammering the database on every request.
    /// </summary>
    [RoutePrefix("api/statistics")]
    public class StatisticsController : ApiController
    {
        private readonly GiveAIDContext _context;

        public StatisticsController(GiveAIDContext context)
        {
            _context = context;
        }

        // GET: api/statistics/overview
        [HttpGet]
        [Route("overview")]
        public async Task<IHttpActionResult> GetOverview()
        {
            var stats = await CacheHelper.GetOrSetAsync("stats_overview", async () =>
            {
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfLastMonth = startOfMonth.AddMonths(-1);

                // Total raised from COMPLETED donations only (financial integrity)
                var totalRaised = await _context.Donations
                    .Where(d => d.PaymentStatus == "Completed")
                    .SumAsync(d => (decimal?)d.Amount) ?? 0m;

                // This month's raised amount
                var thisMonthRaised = await _context.Donations
                    .Where(d => d.PaymentStatus == "Completed" && d.DonationDate >= startOfMonth)
                    .SumAsync(d => (decimal?)d.Amount) ?? 0m;

                // Last month's raised amount (for growth comparison)
                var lastMonthRaised = await _context.Donations
                    .Where(d => d.PaymentStatus == "Completed" &&
                                d.DonationDate >= startOfLastMonth &&
                                d.DonationDate < startOfMonth)
                    .SumAsync(d => (decimal?)d.Amount) ?? 0m;

                // Monthly growth %
                decimal monthlyGrowth = 0;
                if (lastMonthRaised > 0)
                {
                    monthlyGrowth = Math.Round(((thisMonthRaised - lastMonthRaised) / lastMonthRaised) * 100, 1);
                }

                // Total unique donors (users who made at least one completed donation)
                var totalDonors = await _context.Donations
                    .Where(d => d.PaymentStatus == "Completed")
                    .Select(d => d.UserId)
                    .Distinct()
                    .CountAsync();

                // Active campaigns count
                var activeCampaigns = await _context.Campaigns
                    .Where(c => c.Status == "Active")
                    .CountAsync();

                // Total campaigns
                var totalCampaigns = await _context.Campaigns.CountAsync();

                // Completed campaigns
                var completedCampaigns = await _context.Campaigns
                    .Where(c => c.Status == "Completed")
                    .CountAsync();

                // Causes count
                var totalCauses = await _context.Causes
                    .Where(c => c.IsActive)
                    .CountAsync();

                return new DashboardStats
                {
                    TotalRaised = totalRaised,
                    TotalDonors = totalDonors,
                    ActiveCampaigns = activeCampaigns,
                    TotalCampaigns = totalCampaigns,
                    CompletedCampaigns = completedCampaigns,
                    TotalCauses = totalCauses,
                    ThisMonthRaised = thisMonthRaised,
                    LastMonthRaised = lastMonthRaised,
                    MonthlyGrowthPercent = monthlyGrowth,
                    LastUpdated = DateTime.Now
                };
            }, cacheMinutes: 5);

            return Ok(new { success = true, data = stats });
        }

        // GET: api/statistics/dashboard
        [HttpGet]
        [Route("dashboard")]
        public async Task<IHttpActionResult> GetDashboardStatistics()
        {
            // Return cached data using CacheHelper
            var stats = await CacheHelper.GetOrSetAsync("stats_dashboard", async () =>
            {
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfLastMonth = startOfMonth.AddMonths(-1);

                // Total raised from COMPLETED donations only (financial integrity)
                var totalRaised = await _context.Donations
                    .Where(d => d.PaymentStatus == "Completed")
                    .SumAsync(d => (decimal?)d.Amount) ?? 0m;

                // This month's raised amount
                var thisMonthRaised = await _context.Donations
                    .Where(d => d.PaymentStatus == "Completed" && d.DonationDate >= startOfMonth)
                    .SumAsync(d => (decimal?)d.Amount) ?? 0m;

                // Last month's raised amount (for growth comparison)
                var lastMonthRaised = await _context.Donations
                    .Where(d => d.PaymentStatus == "Completed" &&
                                d.DonationDate >= startOfLastMonth &&
                                d.DonationDate < startOfMonth)
                    .SumAsync(d => (decimal?)d.Amount) ?? 0m;

                // Monthly growth %
                decimal monthlyGrowth = 0;
                if (lastMonthRaised > 0)
                {
                    monthlyGrowth = Math.Round(((thisMonthRaised - lastMonthRaised) / lastMonthRaised) * 100, 1);
                }

                // Total unique donors (users who made at least one completed donation)
                var totalDonors = await _context.Donations
                    .Where(d => d.PaymentStatus == "Completed")
                    .Select(d => d.UserId)
                    .Distinct()
                    .CountAsync();

                // Active campaigns count
                var activeCampaigns = await _context.Campaigns
                    .Where(c => c.Status == "Active")
                    .CountAsync();

                // Total campaigns
                var totalCampaigns = await _context.Campaigns.CountAsync();

                // Completed campaigns
                var completedCampaigns = await _context.Campaigns
                    .Where(c => c.Status == "Completed")
                    .CountAsync();

                // Causes count
                var totalCauses = await _context.Causes
                    .Where(c => c.IsActive)
                    .CountAsync();

                return new DashboardStats
                {
                    TotalRaised = totalRaised,
                    TotalDonors = totalDonors,
                    ActiveCampaigns = activeCampaigns,
                    TotalCampaigns = totalCampaigns,
                    CompletedCampaigns = completedCampaigns,
                    TotalCauses = totalCauses,
                    ThisMonthRaised = thisMonthRaised,
                    LastMonthRaised = lastMonthRaised,
                    MonthlyGrowthPercent = monthlyGrowth,
                    LastUpdated = DateTime.Now
                };
            }, cacheMinutes: 5);

            return Ok(stats);
        }

        // GET: api/statistics/campaigns/performance
        [HttpGet]
        [Route("campaigns/performance")]
        public async Task<IHttpActionResult> GetCampaignsPerformance()
        {
            var campaigns = await _context.Campaigns
                .Include(c => c.Cause)
                .Where(c => c.Status == "Active" || c.RaisedAmount > 0)
                .Select(c => new
                {
                    c.CampaignId,
                    CampaignName = c.CampaignName,
                    CauseName = c.Cause.CauseName,
                    c.RaisedAmount,
                    c.GoalAmount,
                    PercentageReached = c.GoalAmount > 0
                        ? Math.Round((c.RaisedAmount / c.GoalAmount) * 100, 1)
                        : 0,
                    c.Status,
                    c.EndDate,
                    DaysRemaining = c.EndDate.HasValue
                        ? (c.EndDate.Value - DateTime.Now).Days >= 0
                            ? (c.EndDate.Value - DateTime.Now).Days
                            : 0
                        : (int?)null,
                    DonorCount = c.Donations.Count(d => d.PaymentStatus == "Completed")
                })
                .OrderByDescending(c => c.RaisedAmount)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = campaigns,
                count = campaigns.Count
            });
        }

        // GET: api/statistics/donations/monthly
        [HttpGet]
        [Route("donations/monthly")]
        public async Task<IHttpActionResult> GetMonthlyDonations()
        {
            var twelveMonthsAgo = DateTime.Now.AddMonths(-12);

            var monthlyData = await _context.Donations
                .Where(d => d.PaymentStatus == "Completed" && d.DonationDate >= twelveMonthsAgo)
                .GroupBy(d => new { d.DonationDate.Year, d.DonationDate.Month })
                .Select(g => new
                {
                    year = g.Key.Year,
                    month = g.Key.Month,
                    monthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
                    total = g.Sum(d => d.Amount),
                    count = g.Count()
                })
                .OrderBy(x => x.year).ThenBy(x => x.month)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = monthlyData,
                period = "last 12 months"
            });
        }

        // GET: api/statistics/top-donors
        [HttpGet]
        [Route("top-donors")]
        public async Task<IHttpActionResult> GetTopDonors()
        {
            var topDonors = await _context.Donations
                .Where(d => d.PaymentStatus == "Completed")
                .GroupBy(d => new { d.UserId, d.User.FullName, d.User.Email })
                .Select(g => new
                {
                    userId = g.Key.UserId,
                    donorName = g.Key.FullName ?? "Anonymous",
                    donorEmail = g.Key.Email ?? "",
                    totalDonated = g.Sum(d => d.Amount),
                    donationCount = g.Count(),
                    lastDonation = g.Max(d => d.DonationDate)
                })
                .OrderByDescending(x => x.totalDonated)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = topDonors,
                count = topDonors.Count
            });
        }

        /// <summary>
        /// Simple in-memory cache model.
        /// </summary>
        private class DashboardStats
        {
            public decimal TotalRaised { get; set; }
            public int TotalDonors { get; set; }
            public int ActiveCampaigns { get; set; }
            public int TotalCampaigns { get; set; }
            public int CompletedCampaigns { get; set; }
            public int TotalCauses { get; set; }
            public decimal ThisMonthRaised { get; set; }
            public decimal LastMonthRaised { get; set; }
            public decimal MonthlyGrowthPercent { get; set; }
            public DateTime LastUpdated { get; set; }
        }
    }
}
