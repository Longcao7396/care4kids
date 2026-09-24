using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class Gallery : BaseEntity
{
    [Key]
    public int GalleryId { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [Required]
    [MaxLength(255)]
    public string PhotoUrl { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ThumbnailUrl { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    [MaxLength(255)]
    public string? Tags { get; set; }

    public int? ProgrammeId { get; set; }
    public int? OrganizationId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }
    public int? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("ProgrammeId")]
    public virtual Programme? Programme { get; set; }

    [ForeignKey("OrganizationId")]
    public virtual Organization? Organization { get; set; }
}
