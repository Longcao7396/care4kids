using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class Achievement : BaseEntity
{
    [Key]
    public int AchievementId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }

    public string? Description { get; set; }
    public decimal? MetricValue { get; set; }

    [MaxLength(100)]
    public string? MetricLabel { get; set; }

    [MaxLength(20)]
    public string? MetricSuffix { get; set; }

    public DateTime? AchievementDate { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    [MaxLength(150)]
    public string? AwardBy { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public int? Beneficiaries { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int? CreatedBy { get; set; }

    // Navigation properties
    [ForeignKey("CreatedBy")]
    public virtual User? CreatedByUser { get; set; }
}
