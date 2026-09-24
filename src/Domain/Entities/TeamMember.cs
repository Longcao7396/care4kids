using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

[Table("team_members")]
public class TeamMember : BaseEntity
{
    [Key]
    public int TeamMemberId { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string RoleTitle { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Department { get; set; }

    public string? Bio { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? LinkedInUrl { get; set; }

    [MaxLength(255)]
    public string? TwitterUrl { get; set; }

    [MaxLength(255)]
    public string? FacebookUrl { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public DateTime? JoinedDate { get; set; }
    public int? CreatedBy { get; set; }

    // Navigation properties
    [ForeignKey("CreatedBy")]
    public virtual User? CreatedByUser { get; set; }
}
