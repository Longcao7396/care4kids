using GiveAID.Application.Features.Causes.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Causes.Queries.GetTree;

/// <summary>
/// Handler for GetCauseTreeQuery.
/// </summary>
public class GetCauseTreeQueryHandler : IRequestHandler<GetCauseTreeQuery, IEnumerable<CauseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCauseTreeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CauseDto>> Handle(GetCauseTreeQuery request, CancellationToken cancellationToken)
    {
        var allCauses = await _context.Causes
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.CauseName)
            .ToListAsync(cancellationToken);

        // Get parent causes only
        var parentCauses = allCauses.Where(c => c.ParentCauseId == null);

        return parentCauses.Select(parent => MapToDtoWithChildren(parent, allCauses));
    }

    private CauseDto MapToDtoWithChildren(Domain.Entities.Cause parent, List<Domain.Entities.Cause> allCauses)
    {
        var dto = new CauseDto
        {
            CauseId = parent.CauseId,
            CauseCode = parent.CauseCode,
            CauseName = parent.CauseName,
            Description = parent.Description,
            ImageUrl = parent.ImageUrl,
            Icon = parent.Icon,
            TargetAmount = parent.TargetAmount,
            RaisedAmount = parent.RaisedAmount,
            PercentageReached = parent.TargetAmount > 0 ? (parent.RaisedAmount / parent.TargetAmount) * 100 : 0,
            IsActive = parent.IsActive,
            DisplayOrder = parent.DisplayOrder,
            ParentCauseId = parent.ParentCauseId,
            IsParentCause = true,
            SubCauses = new List<CauseDto>()
        };

        var subCauses = allCauses.Where(c => c.ParentCauseId == parent.CauseId);
        foreach (var sub in subCauses)
        {
            dto.SubCauses.Add(new CauseDto
            {
                CauseId = sub.CauseId,
                CauseCode = sub.CauseCode,
                CauseName = sub.CauseName,
                Description = sub.Description,
                ImageUrl = sub.ImageUrl,
                Icon = sub.Icon,
                TargetAmount = sub.TargetAmount,
                RaisedAmount = sub.RaisedAmount,
                PercentageReached = sub.TargetAmount > 0 ? (sub.RaisedAmount / sub.TargetAmount) * 100 : 0,
                IsActive = sub.IsActive,
                DisplayOrder = sub.DisplayOrder,
                ParentCauseId = sub.ParentCauseId,
                IsParentCause = false
            });
        }

        return dto;
    }
}
