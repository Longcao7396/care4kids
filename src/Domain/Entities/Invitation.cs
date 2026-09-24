using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class Invitation : BaseEntity
{
    [Key]
    public int InvitationId { get; set; }

    public int? InviterUserId { get; set; }

    [Required]
    [MaxLength(150)]
    public string InviteeName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string InviteeEmail { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? PersonalMessage { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [MaxLength(64)]
    public string? InvitationToken { get; set; }

    public DateTime? SentAt { get; set; }
    public DateTime? RegisteredAt { get; set; }
    public string? FailureReason { get; set; }

    // Navigation properties
    [ForeignKey("InviterUserId")]
    public virtual User? Inviter { get; set; }
}
