using GiveAID.Application.Features.Achievements.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Achievements.Queries.GetAll;

/// <summary>
/// Query to get all achievements.
/// </summary>
public class GetAllAchievementsQuery : IRequest<IEnumerable<AchievementDto>>
{
    public bool ActiveOnly { get; set; } = true;
}
