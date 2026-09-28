using GiveAID.Application.Features.Gallery.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Gallery.Queries.GetFeatured;

/// <summary>
/// Handler for GetFeaturedGalleryQuery.
/// </summary>
public class GetFeaturedGalleryQueryHandler : IRequestHandler<GetFeaturedGalleryQuery, IEnumerable<GalleryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFeaturedGalleryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GalleryDto>> Handle(GetFeaturedGalleryQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.Gallery
            .Include(g => g.Organization)
            .Where(g => g.IsFeatured)
            .OrderBy(g => g.DisplayOrder)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return items.Select(g => new GalleryDto
        {
            GalleryId = g.GalleryId,
            Title = g.Title,
            PhotoUrl = g.PhotoUrl,
            ThumbnailUrl = g.ThumbnailUrl,
            Category = g.Category,
            Tags = g.Tags,
            OrganizationId = g.OrganizationId,
            OrganizationName = g.Organization?.OrganizationName,
            DisplayOrder = g.DisplayOrder,
            IsFeatured = g.IsFeatured,
            UploadedAt = g.UploadedAt,
            PublicId = g.PublicId,
            OriginalFileName = g.OriginalFileName,
            FileSizeBytes = g.FileSizeBytes,
            ContentType = g.ContentType
        });
    }
}
