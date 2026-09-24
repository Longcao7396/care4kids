using GiveAID.Application.Features.Causes.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Causes.Queries.GetAll;

/// <summary>
/// Handler for GetAllCausesQuery.
/// </summary>
public class GetAllCausesQueryHandler : IRequestHandler<GetAllCausesQuery, IEnumerable<CauseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCausesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CauseDto>> Handle(GetAllCausesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Causes.AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(c => c.IsActive);
        }

        var causes = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.CauseName)
            .ToListAsync(cancellationToken);

        return causes.Select(MapToDto);
    }

    private CauseDto MapToDto(Domain.Entities.Cause cause)
    {
        return new CauseDto
        {
            CauseId = cause.CauseId,
            CauseCode = cause.CauseCode,
            CauseName = cause.CauseName,
            Description = cause.Description,
            ImageUrl = cause.ImageUrl,
            Icon = cause.Icon,
            TargetAmount = cause.TargetAmount,
            RaisedAmount = cause.RaisedAmount,
            PercentageReached = cause.TargetAmount > 0 ? (cause.RaisedAmount / cause.TargetAmount) * 100 : 0,
            IsActive = cause.IsActive,
            DisplayOrder = cause.DisplayOrder,
            ParentCauseId = cause.ParentCauseId,
            IsParentCause = cause.ParentCauseId == null
        };
    }
}
