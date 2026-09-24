using GiveAID.Application.Features.Achievements.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Achievements.Queries.GetAll;

/// <summary>
/// Handler for GetAllAchievementsQuery.
/// </summary>
public class GetAllAchievementsQueryHandler : IRequestHandler<GetAllAchievementsQuery, IEnumerable<AchievementDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllAchievementsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AchievementDto>> Handle(GetAllAchievementsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Achievements.AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(a => a.IsActive);
        }

        var achievements = await query
            .OrderBy(a => a.DisplayOrder)
            .ThenByDescending(a => a.AchievementDate)
            .ToListAsync(cancellationToken);

        return achievements.Select(MapToDto);
    }

    private AchievementDto MapToDto(Domain.Entities.Achievement a)
    {
        return new AchievementDto
        {
            AchievementId = a.AchievementId,
            Title = a.Title,
            Category = a.Category,
            Description = a.Description,
            MetricValue = a.MetricValue,
            MetricLabel = a.MetricLabel,
            MetricSuffix = a.MetricSuffix,
            AchievementDate = a.AchievementDate,
            ImageUrl = a.ImageUrl,
            Icon = a.Icon,
            AwardBy = a.AwardBy,
            Location = a.Location,
            Beneficiaries = a.Beneficiaries,
            IsActive = a.IsActive,
            IsFeatured = a.IsFeatured,
            DisplayOrder = a.DisplayOrder
        };
    }
}
