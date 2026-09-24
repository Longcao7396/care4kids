using GiveAID.Application.Features.Achievements.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Achievements.Commands.Update;

/// <summary>
/// Command to update an achievement.
/// </summary>
public class UpdateAchievementCommand : IRequest<AchievementDto>
{
    public int AchievementId { get; set; }
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal? MetricValue { get; set; }
    public string? MetricLabel { get; set; }
    public string? MetricSuffix { get; set; }
    public DateTime? AchievementDate { get; set; }
    public string? ImageUrl { get; set; }
    public string? Icon { get; set; }
    public string? AwardBy { get; set; }
    public string? Location { get; set; }
    public int? Beneficiaries { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
    public int? DisplayOrder { get; set; }
}
