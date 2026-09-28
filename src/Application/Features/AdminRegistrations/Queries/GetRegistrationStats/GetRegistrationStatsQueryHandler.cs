using GiveAID.Application.Common.Interfaces;
using GiveAID.Application.Features.AdminRegistrations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.AdminRegistrations.Queries.GetRegistrationStats;

public class GetRegistrationStatsQueryHandler
    : IRequestHandler<GetRegistrationStatsQuery, AdminRegistrationStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetRegistrationStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminRegistrationStatsDto> Handle(
        GetRegistrationStatsQuery request,
        CancellationToken cancellationToken)
    {
        var campaignStatuses = await _context.CampaignRegistrations
            .AsNoTracking()
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var programmeStatuses = await _context.ProgrammeRegistrations
            .AsNoTracking()
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        int Count(string s) => campaignStatuses.FirstOrDefault(x => x.Status == s)?.Count ?? 0;
        int CountP(string s) => programmeStatuses.FirstOrDefault(x => x.Status == s)?.Count ?? 0;

        // Treat "Registered" as Pending because the existing registration
        // command always creates rows with Status = "Registered". Admins see
        // these as needing approval.
        var pending = Count("Pending") + Count("Registered") + CountP("Pending") + CountP("Registered");
        var approved = Count("Approved") + CountP("Approved");
        var rejected = Count("Rejected") + CountP("Rejected");
        var cancelled = Count("Cancelled") + CountP("Cancelled");

        return new AdminRegistrationStatsDto
        {
            Total = pending + approved + rejected + cancelled,
            Pending = pending,
            Approved = approved,
            Rejected = rejected,
            Cancelled = cancelled,
            CampaignCount = campaignStatuses.Sum(x => x.Count),
            ProgrammeCount = programmeStatuses.Sum(x => x.Count),
        };
    }
}