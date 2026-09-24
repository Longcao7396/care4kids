namespace GiveAID.Application.Features.Invitations.DTOs;

/// <summary>
/// DTO for invitation data.
/// </summary>
public class InvitationDto
{
    public int InvitationId { get; set; }
    public int? InviterUserId { get; set; }
    public string? InviterName { get; set; }
    public string InviteeName { get; set; } = string.Empty;
    public string InviteeEmail { get; set; } = string.Empty;
    public string? PersonalMessage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? InvitationToken { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? RegisteredAt { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
