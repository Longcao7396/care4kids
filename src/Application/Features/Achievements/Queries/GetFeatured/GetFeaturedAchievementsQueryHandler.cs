using GiveAID.Application.Features.Achievements.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Achievements.Queries.GetFeatured;

/// <summary>
/// Handler for GetFeaturedAchievementsQuery.
/// </summary>
public class GetFeaturedAchievementsQueryHandler : IRequestHandler<GetFeaturedAchievementsQuery, IEnumerable<AchievementDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFeaturedAchievementsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AchievementDto>> Handle(GetFeaturedAchievementsQuery request, CancellationToken cancellationToken)
    {
        var achievements = await _context.Achievements
            .Where(a => a.IsFeatured && a.IsActive)
            .OrderBy(a => a.DisplayOrder)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return achievements.Select(a => new AchievementDto
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
        });
    }
}
