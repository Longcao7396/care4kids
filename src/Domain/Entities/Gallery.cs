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
    [MaxLength(500)] // Increased from 255 — Cloudinary URLs can be longer (e.g., with version + folder)
    public string PhotoUrl { get; set; } = string.Empty;

    // DEPRECATED: ThumbnailUrl is now generated on-the-fly by Cloudinary transform.
    // The column is kept temporarily for backward compat with existing DB rows.
    // Will be dropped in a later cleanup migration.
    [MaxLength(500)]
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

    // ===== Cloudinary upload metadata (NEW — migration: AddImageUploadFields) =====
    // PublicId identifies the file in Cloudinary so we can delete/replace later.
    [MaxLength(255)]
    public string? PublicId { get; set; }

    // Original file name shown in admin list (e.g., "lớp-học-vùng-cao.jpg")
    [MaxLength(255)]
    public string? OriginalFileName { get; set; }

    // File size in bytes — for quota tracking + display.
    public long? FileSizeBytes { get; set; }

    // MIME type at upload time (e.g., "image/jpeg").
    [MaxLength(50)]
    public string? ContentType { get; set; }

    // Navigation properties
    [ForeignKey("ProgrammeId")]
    public virtual Programme? Programme { get; set; }

    [ForeignKey("OrganizationId")]
    public virtual Organization? Organization { get; set; }
}
