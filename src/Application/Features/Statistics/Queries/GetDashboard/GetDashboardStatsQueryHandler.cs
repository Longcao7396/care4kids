using GiveAID.Application.Features.Statistics.DTOs;
using GiveAID.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Statistics.Queries.GetDashboard;

/// <summary>
/// Handler for GetDashboardStatsQuery.
/// </summary>
public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public GetDashboardStatsQueryHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // Bug #2 fix: cache key now versioned so previously cached responses
        // (from before DonationsByMonth existed) are never served stale/incomplete.
        var cacheKey = "dashboard_stats_v2";
        var cached = _cacheService.Get<DashboardStatsDto>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var today = DateTime.UtcNow.Date;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var completedDonations = _context.Donations.Where(d => d.PaymentStatus == "Completed");

        var totalRaised = await completedDonations.SumAsync(d => d.Amount, cancellationToken);
        var totalRaisedToday = await completedDonations.Where(d => d.DonationDate >= today).SumAsync(d => d.Amount, cancellationToken);
        var totalRaisedThisMonth = await completedDonations.Where(d => d.DonationDate >= startOfMonth).SumAsync(d => d.Amount, cancellationToken);

        var donationsToday = await completedDonations.CountAsync(d => d.DonationDate >= today, cancellationToken);
        var donationsThisMonth = await completedDonations.CountAsync(d => d.DonationDate >= startOfMonth, cancellationToken);

        var totalDonors = await _context.Donations
            .Where(d => d.PaymentStatus == "Completed")
            .Select(d => d.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalCampaigns = await _context.Campaigns.CountAsync(cancellationToken);
        var activeCampaigns = await _context.Campaigns.CountAsync(c => c.Status == "Active", cancellationToken);
        var totalCauses = await _context.Causes.CountAsync(c => c.IsActive, cancellationToken);
        var registeredUsers = await _context.Users.CountAsync(u => u.IsActive, cancellationToken);

        var recentDonations = await completedDonations
            .OrderByDescending(d => d.DonationDate)
            .Take(10)
            .Select(d => new RecentDonationDto
            {
                DonationId = d.DonationId,
                Amount = d.Amount,
                DonorName = d.IsAnonymous ? "Anonymous" : d.User!.FullName,
                CampaignName = d.Campaign!.CampaignName,
                CauseName = d.Cause!.CauseName,
                DonationDate = d.DonationDate,
                IsAnonymous = d.IsAnonymous
            })
            .ToListAsync(cancellationToken);

        // Bug #2 fix: aggregate donations by month via SQL GROUP BY (no in-memory
        // load of all rows). Covers the last 6 months, oldest to newest.
        var sixMonthsAgoStart = new DateTime(startOfMonth.Year, startOfMonth.Month, 1).AddMonths(-5);
        var monthlyGroups = await completedDonations
            .Where(d => d.DonationDate >= sixMonthsAgoStart)
            .GroupBy(d => new { d.DonationDate.Year, d.DonationDate.Month })
            .Select(g => new MonthlyDonationDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Total = g.Sum(d => d.Amount),
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        // Fill in any months with zero donations so the chart always shows 6 points.
        var donationsByMonth = new List<MonthlyDonationDto>();
        for (var i = 5; i >= 0; i--)
        {
            var monthDate = startOfMonth.AddMonths(-i);
            var match = monthlyGroups.FirstOrDefault(m => m.Year == monthDate.Year && m.Month == monthDate.Month);
            donationsByMonth.Add(match ?? new MonthlyDonationDto
            {
                Year = monthDate.Year,
                Month = monthDate.Month,
                Total = 0,
                Count = 0
            });
        }

        var stats = new DashboardStatsDto
        {
            TotalDonations = totalRaised,
            TotalDonors = totalDonors,
            TotalCampaigns = totalCampaigns,
            ActiveCampaigns = activeCampaigns,
            TotalCauses = totalCauses,
            RegisteredUsers = registeredUsers,
            TotalRaisedThisMonth = totalRaisedThisMonth,
            TotalRaisedToday = totalRaisedToday,
            DonationsToday = donationsToday,
            DonationsThisMonth = donationsThisMonth,
            RecentDonations = recentDonations,
            DonationsByMonth = donationsByMonth
        };

        _cacheService.Set(cacheKey, stats, TimeSpan.FromMinutes(5));

        return stats;
    }
}
