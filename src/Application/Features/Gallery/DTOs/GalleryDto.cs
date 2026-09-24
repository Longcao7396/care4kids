namespace GiveAID.Application.Features.Gallery.DTOs;

/// <summary>
/// DTO for gallery data.
/// </summary>
public class GalleryDto
{
    public int GalleryId { get; set; }
    public string? Title { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime UploadedAt { get; set; }
}
