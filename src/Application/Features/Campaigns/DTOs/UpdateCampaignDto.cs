namespace GiveAID.Application.Features.Campaigns.DTOs;

/// <summary>
/// DTO for updating a campaign.
/// </summary>
public class UpdateCampaignDto
{
    public int CampaignId { get; set; }
    public int? CauseId { get; set; }
    public int? OrganizationId { get; set; }
    public string? CampaignName { get; set; }
    public string? CampaignCode { get; set; }
    public string? ProgrammeType { get; set; }
    public bool? RegistrationRequired { get; set; }
    public int? MaxParticipants { get; set; }
    public int? TargetBeneficiaries { get; set; }
    public decimal? ExpectedBudget { get; set; }
    public decimal? ActualBudget { get; set; }
    public string? Description { get; set; }
    public decimal? GoalAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ImageUrl { get; set; }
    public int? BeneficiariesCount { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
    public bool? IsFeatured { get; set; }
    public int? DisplayOrder { get; set; }
}
