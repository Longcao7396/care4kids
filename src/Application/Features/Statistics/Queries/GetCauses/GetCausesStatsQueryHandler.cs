using GiveAID.Application.Features.Statistics.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Statistics.Queries.GetCauses;

/// <summary>
/// Handler for GetCausesStatsQuery.
/// Uses SQL aggregation to count causes without loading them into memory.
/// </summary>
public class GetCausesStatsQueryHandler : IRequestHandler<GetCausesStatsQuery, CauseStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetCausesStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CauseStatsDto> Handle(GetCausesStatsQuery request, CancellationToken cancellationToken)
    {
        var totalCauses = await _context.Causes.CountAsync(cancellationToken);
        var activeCauses = await _context.Causes.CountAsync(c => c.IsActive, cancellationToken);

        return new CauseStatsDto
        {
            TotalCauses = totalCauses,
            ActiveCauses = activeCauses
        };
    }
}
