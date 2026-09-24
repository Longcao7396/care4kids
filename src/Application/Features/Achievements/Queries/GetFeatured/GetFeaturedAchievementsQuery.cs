using GiveAID.Application.Features.Achievements.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Achievements.Queries.GetFeatured;

/// <summary>
/// Query to get featured achievements.
/// </summary>
public class GetFeaturedAchievementsQuery : IRequest<IEnumerable<AchievementDto>>
{
    public int Limit { get; set; } = 6;
}
