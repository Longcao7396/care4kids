using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class CareerApplication : BaseEntity
{
    [Key]
    public int ApplicationId { get; set; }

    [Required]
    public int CareerId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ApplicantName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? ResumeUrl { get; set; }

    public string? CoverLetter { get; set; }

    [MaxLength(200)]
    public string? LinkedInUrl { get; set; }

    [MaxLength(200)]
    public string? PortfolioUrl { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Submitted";

    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("CareerId")]
    public virtual Career? Career { get; set; }
}
