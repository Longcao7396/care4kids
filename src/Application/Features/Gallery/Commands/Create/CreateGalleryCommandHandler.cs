using GiveAID.Application.Features.Gallery.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Gallery.Commands.Create;

/// <summary>
/// Handler for CreateGalleryCommand.
/// </summary>
public class CreateGalleryCommandHandler : IRequestHandler<CreateGalleryCommand, GalleryDto>
{
    private readonly IApplicationDbContext _context;

    public CreateGalleryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GalleryDto> Handle(CreateGalleryCommand request, CancellationToken cancellationToken)
    {
        var gallery = new Domain.Entities.Gallery
        {
            Title = request.Title,
            PhotoUrl = request.PhotoUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            Category = request.Category,
            Tags = request.Tags,
            OrganizationId = request.OrganizationId,
            ProgrammeId = request.ProgrammeId,
            DisplayOrder = request.DisplayOrder,
            IsFeatured = request.IsFeatured,
            UploadedBy = request.UploadedBy,
            UploadedAt = DateTime.UtcNow,
            // Cloudinary metadata (may be null when going through URL paste path)
            PublicId = request.PublicId,
            OriginalFileName = request.OriginalFileName,
            FileSizeBytes = request.FileSizeBytes,
            ContentType = request.ContentType
        };

        _context.Gallery.Add(gallery);
        await _context.SaveChangesAsync(cancellationToken);

        string? orgName = null;
        if (gallery.OrganizationId.HasValue)
        {
            var org = await _context.Organizations.FindAsync(new object[] { gallery.OrganizationId.Value }, cancellationToken);
            orgName = org?.OrganizationName;
        }

        return new GalleryDto
        {
            GalleryId = gallery.GalleryId,
            Title = gallery.Title,
            PhotoUrl = gallery.PhotoUrl,
            ThumbnailUrl = gallery.ThumbnailUrl,
            Category = gallery.Category,
            Tags = gallery.Tags,
            OrganizationId = gallery.OrganizationId,
            OrganizationName = orgName,
            DisplayOrder = gallery.DisplayOrder,
            IsFeatured = gallery.IsFeatured,
            UploadedAt = gallery.UploadedAt,
            PublicId = gallery.PublicId,
            OriginalFileName = gallery.OriginalFileName,
            FileSizeBytes = gallery.FileSizeBytes,
            ContentType = gallery.ContentType
        };
    }
}
