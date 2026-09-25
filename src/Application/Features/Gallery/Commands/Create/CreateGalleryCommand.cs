using GiveAID.Application.Features.Gallery.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.Gallery.Commands.Create;

/// <summary>
/// Command to create a gallery item. Supports both file upload and URL-based creation.
/// </summary>
public class CreateGalleryCommand : IRequest<GalleryDto>
{
    public string? Title { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public int? OrganizationId { get; set; }
    public int? ProgrammeId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }
    public int? UploadedBy { get; set; }

    // NEW — populated when upload goes through IImageStorageService
    public string? PublicId { get; set; }
    public string? OriginalFileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? ContentType { get; set; }
}
