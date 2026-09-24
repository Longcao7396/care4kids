using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class ProgrammePhoto : BaseEntity
{
    [Key]
    public int PhotoId { get; set; }

    [Required]
    public int ProgrammeId { get; set; }

    [Required]
    [MaxLength(255)]
    public string PhotoUrl { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Caption { get; set; }

    public int DisplayOrder { get; set; }
    public int? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("ProgrammeId")]
    public virtual Programme? Programme { get; set; }
}
