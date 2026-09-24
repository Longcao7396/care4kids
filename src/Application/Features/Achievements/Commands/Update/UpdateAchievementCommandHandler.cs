using GiveAID.Application.Features.Achievements.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Achievements.Commands.Update;

/// <summary>
/// Handler for UpdateAchievementCommand.
/// </summary>
public class UpdateAchievementCommandHandler : IRequestHandler<UpdateAchievementCommand, AchievementDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateAchievementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AchievementDto> Handle(UpdateAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _context.Achievements.FindAsync(new object[] { request.AchievementId }, cancellationToken);

        if (achievement == null)
        {
            throw new InvalidOperationException($"Achievement with ID {request.AchievementId} not found.");
        }

        if (request.Title != null) achievement.Title = request.Title;
        if (request.Category != null) achievement.Category = request.Category;
        if (request.Description != null) achievement.Description = request.Description;
        if (request.MetricValue.HasValue) achievement.MetricValue = request.MetricValue;
        if (request.MetricLabel != null) achievement.MetricLabel = request.MetricLabel;
        if (request.MetricSuffix != null) achievement.MetricSuffix = request.MetricSuffix;
        if (request.AchievementDate.HasValue) achievement.AchievementDate = request.AchievementDate;
        if (request.ImageUrl != null) achievement.ImageUrl = request.ImageUrl;
        if (request.Icon != null) achievement.Icon = request.Icon;
        if (request.AwardBy != null) achievement.AwardBy = request.AwardBy;
        if (request.Location != null) achievement.Location = request.Location;
        if (request.Beneficiaries.HasValue) achievement.Beneficiaries = request.Beneficiaries;
        if (request.IsActive.HasValue) achievement.IsActive = request.IsActive.Value;
        if (request.IsFeatured.HasValue) achievement.IsFeatured = request.IsFeatured.Value;
        if (request.DisplayOrder.HasValue) achievement.DisplayOrder = request.DisplayOrder.Value;
        achievement.UpdatedAt = DateTime.UtcNow;

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
