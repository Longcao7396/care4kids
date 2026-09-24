using GiveAID.Application.Features.Causes.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Causes.Queries.GetById;

/// <summary>
/// Handler for GetCauseByIdQuery.
/// </summary>
public class GetCauseByIdQueryHandler : IRequestHandler<GetCauseByIdQuery, CauseDto>
{
    private readonly IApplicationDbContext _context;

    public GetCauseByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CauseDto> Handle(GetCauseByIdQuery request, CancellationToken cancellationToken)
    {
        var cause = await _context.Causes
            .Include(c => c.SubCauses)
            .FirstOrDefaultAsync(c => c.CauseId == request.CauseId, cancellationToken);

        if (cause == null)
        {
            throw new InvalidOperationException($"Cause with ID {request.CauseId} not found.");
        }

        var dto = new CauseDto
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
            IsParentCause = cause.ParentCauseId == null,
            SubCauses = cause.SubCauses?.Select(s => new CauseDto
            {
                CauseId = s.CauseId,
                CauseCode = s.CauseCode,
                CauseName = s.CauseName,
                Description = s.Description,
                ImageUrl = s.ImageUrl,
                Icon = s.Icon,
                TargetAmount = s.TargetAmount,
                RaisedAmount = s.RaisedAmount,
                PercentageReached = s.TargetAmount > 0 ? (s.RaisedAmount / s.TargetAmount) * 100 : 0,
                IsActive = s.IsActive,
                DisplayOrder = s.DisplayOrder,
                ParentCauseId = s.ParentCauseId,
                IsParentCause = false
            }).ToList()
        };

        return dto;
    }
}
