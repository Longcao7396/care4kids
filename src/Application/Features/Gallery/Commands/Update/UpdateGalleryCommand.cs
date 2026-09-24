using GiveAID.Application.Features.Gallery.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Gallery.Commands.Update;

/// <summary>
/// Command to update a gallery item.
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
}
