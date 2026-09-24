using GiveAID.Application.Features.Gallery.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.Gallery.Commands.Create;

/// <summary>
/// Command to create a gallery item.
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
}
