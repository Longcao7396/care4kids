namespace GiveAID.Application.Features.Achievements.DTOs;

/// <summary>
/// DTO for achievement data.
/// </summary>
public class AchievementDto
{
    public int AchievementId { get; set; }
    public string Title { get; set; } = string.Empty;
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
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
}
