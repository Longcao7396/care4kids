using MediatR;

namespace GiveAID.Application.Features.Achievements.Commands.Delete;

/// <summary>
/// Command to delete an achievement.
/// </summary>
public class DeleteAchievementCommand : IRequest<bool>
{
    public int AchievementId { get; set; }
}
