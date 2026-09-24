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
        var cacheKey = "dashboard_stats";
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
            RecentDonations = recentDonations
        };

        _cacheService.Set(cacheKey, stats, TimeSpan.FromMinutes(5));

        return stats;
    }
}
