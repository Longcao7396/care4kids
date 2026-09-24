using GiveAID.Application.Features.Achievements.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.Achievements.Commands.Create;

/// <summary>
/// Handler for CreateAchievementCommand.
/// </summary>
public class CreateAchievementCommandHandler : IRequestHandler<CreateAchievementCommand, AchievementDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAchievementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AchievementDto> Handle(CreateAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = new Achievement
        {
            Title = request.Title,
            Category = request.Category,
            Description = request.Description,
            MetricValue = request.MetricValue,
            MetricLabel = request.MetricLabel,
            MetricSuffix = request.MetricSuffix,
            AchievementDate = request.AchievementDate,
            ImageUrl = request.ImageUrl,
            Icon = request.Icon,
            AwardBy = request.AwardBy,
            Location = request.Location,
            Beneficiaries = request.Beneficiaries,
            IsFeatured = request.IsFeatured,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Achievements.Add(achievement);
        await _context.SaveChangesAsync(cancellationToken);

        return new AchievementDto
        {
            AchievementId = achievement.AchievementId,
            Title = achievement.Title,
            Category = achievement.Category,
            Description = achievement.Description,
            MetricValue = achievement.MetricValue,
            MetricLabel = achievement.MetricLabel,
            MetricSuffix = achievement.MetricSuffix,
            AchievementDate = achievement.AchievementDate,
            ImageUrl = achievement.ImageUrl,
            Icon = achievement.Icon,
            AwardBy = achievement.AwardBy,
            Location = achievement.Location,
            Beneficiaries = achievement.Beneficiaries,
            IsActive = achievement.IsActive,
            IsFeatured = achievement.IsFeatured,
            DisplayOrder = achievement.DisplayOrder
        };
    }
}
