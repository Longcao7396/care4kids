using GiveAID.Application.Features.Gallery.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Gallery.Commands.Update;

/// <summary>
/// Command to update a gallery item. Supports both file upload and URL-based update.
/// </summary>
public class UpdateGalleryCommand : IRequest<GalleryDto>
{
    public int GalleryId { get; set; }
    public string? Title { get; set; }
    public string? PhotoUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public int? OrganizationId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }

    // NEW — set when file replaces existing image
    public string? PublicId { get; set; }
    public string? OriginalFileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? ContentType { get; set; }

    // Indicates that this update replaces the existing file (not just metadata).
    // When true, handler will delete the OLD file from Cloudinary.
    public bool ReplacingFile { get; set; }
}
