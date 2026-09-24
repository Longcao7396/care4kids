namespace GiveAID.Application.Features.CampaignRegistrations.DTOs;

/// <summary>
/// DTO for campaign registration data.
/// </summary>
public class RegistrationDto
{
    public int RegistrationId { get; set; }
    public int CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool AttendanceConfirmed { get; set; }
    public DateTime RegistrationDate { get; set; }
}
