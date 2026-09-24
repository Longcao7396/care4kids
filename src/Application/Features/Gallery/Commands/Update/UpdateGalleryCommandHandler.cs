using GiveAID.Application.Features.Gallery.DTOs;

namespace GiveAID.Application.Features.Gallery.Commands.Update;

/// <summary>
/// Handler for UpdateGalleryCommand.
/// </summary>
public class UpdateGalleryCommandHandler : IRequestHandler<UpdateGalleryCommand, GalleryDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateGalleryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GalleryDto> Handle(UpdateGalleryCommand request, CancellationToken cancellationToken)
    {
        var gallery = await _context.Gallery.FindAsync(new object[] { request.GalleryId }, cancellationToken);

        if (gallery == null)
        {
            throw new InvalidOperationException($"Gallery item with ID {request.GalleryId} not found.");
        }

        if (request.Title != null) gallery.Title = request.Title;
        if (request.PhotoUrl != null) gallery.PhotoUrl = request.PhotoUrl;
        if (request.ThumbnailUrl != null) gallery.ThumbnailUrl = request.ThumbnailUrl;
        if (request.Category != null) gallery.Category = request.Category;
        if (request.Tags != null) gallery.Tags = request.Tags;
        if (request.OrganizationId.HasValue) gallery.OrganizationId = request.OrganizationId;
        gallery.DisplayOrder = request.DisplayOrder;
        gallery.IsFeatured = request.IsFeatured;

        await _context.SaveChangesAsync(cancellationToken);

        return new GalleryDto
        {
            GalleryId = gallery.GalleryId,
            Title = gallery.Title,
            PhotoUrl = gallery.PhotoUrl,
            ThumbnailUrl = gallery.ThumbnailUrl,
            Category = gallery.Category,
            Tags = gallery.Tags,
            OrganizationId = gallery.OrganizationId,
            DisplayOrder = gallery.DisplayOrder,
            IsFeatured = gallery.IsFeatured,
            UploadedAt = gallery.UploadedAt
        };
    }
}
