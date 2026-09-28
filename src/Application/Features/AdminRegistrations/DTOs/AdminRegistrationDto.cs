namespace GiveAID.Application.Features.AdminRegistrations.DTOs;

/// <summary>
/// Unified DTO returned to admin for either a campaign or programme registration.
/// </summary>
public class AdminRegistrationDto
{
    public int RegistrationId { get; set; }

    /// <summary>"Campaign" or "Programme".</summary>
    public string RegistrationType { get; set; } = string.Empty;

    public int CampaignId { get; set; }
    public string? CampaignName { get; set; }

    public int ProgrammeId { get; set; }
    public string? ProgrammeName { get; set; }

    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }

    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool AttendanceConfirmed { get; set; }
    public DateTime RegistrationDate { get; set; }

    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public string? RejectionReason { get; set; }
}