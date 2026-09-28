using MediatR;

namespace GiveAID.Application.Features.Achievements.Commands.Delete;

/// <summary>
/// Handler for DeleteAchievementCommand.
/// </summary>
public class DeleteAchievementCommandHandler : IRequestHandler<DeleteAchievementCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteAchievementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _context.Achievements.FindAsync(new object[] { request.AchievementId }, cancellationToken);

        if (achievement == null)
        {
            throw new InvalidOperationException($"Achievement with ID {request.AchievementId} not found.");
        }

        achievement.IsDeleted = true;
        achievement.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
