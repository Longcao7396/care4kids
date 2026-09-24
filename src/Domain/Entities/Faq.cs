using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class Faq : BaseEntity
{
    [Key]
    public int FaqId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Answer { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
    public int? CreatedBy { get; set; }

    // Navigation properties
    [ForeignKey("CreatedBy")]
    public virtual User? CreatedByUser { get; set; }
}
