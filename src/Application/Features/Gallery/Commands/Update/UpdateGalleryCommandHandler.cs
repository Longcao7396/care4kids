using GiveAID.Application.Common.Interfaces;
using GiveAID.Application.Features.Gallery.DTOs;
using Microsoft.Extensions.Logging;

namespace GiveAID.Application.Features.Gallery.Commands.Update;

/// <summary>
/// Handler for UpdateGalleryCommand.
/// When ReplacingFile=true, deletes the old image from Cloudinary to avoid orphaned files.
/// </summary>
public class UpdateGalleryCommandHandler : IRequestHandler<UpdateGalleryCommand, GalleryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IImageStorageService _imageStorage;
    private readonly ILogger<UpdateGalleryCommandHandler> _logger;

    public UpdateGalleryCommandHandler(
        IApplicationDbContext context,
        IImageStorageService imageStorage,
        ILogger<UpdateGalleryCommandHandler> logger)
    {
        _context = context;
        _imageStorage = imageStorage;
        _logger = logger;
    }

    public async Task<GalleryDto> Handle(UpdateGalleryCommand request, CancellationToken cancellationToken)
    {
        var gallery = await _context.Gallery.FindAsync(new object[] { request.GalleryId }, cancellationToken);

        if (gallery == null)
        {
            throw new InvalidOperationException($"Gallery item with ID {request.GalleryId} not found.");
        }

        // ===== Delete old file from Cloudinary if replacing =====
        if (request.ReplacingFile && !string.IsNullOrEmpty(gallery.PublicId))
        {
            var oldPublicId = gallery.PublicId;
            var oldPhotoUrl = gallery.PhotoUrl;

            try
            {
                var deleted = await _imageStorage.DeleteAsync(oldPublicId, cancellationToken);
                if (deleted)
                {
                    _logger.LogInformation(
                        "Deleted old image from Cloudinary during Gallery update: galleryId={GalleryId}, publicId={PublicId}, oldUrl={OldUrl}",
                        gallery.GalleryId,
                        oldPublicId,
                        oldPhotoUrl);
                }
                else
                {
                    // Delete returned false — file may not exist on Cloudinary (already deleted manually?),
                    // but DB still had a reference. Log a Warning so this is recoverable.
                    _logger.LogWarning(
                        "Cloudinary DeleteAsync returned false during Gallery update. File may already be absent. " +
                        "Manual cleanup may be needed. galleryId={GalleryId}, publicId={PublicId}, oldUrl={OldUrl}",
                        gallery.GalleryId,
                        oldPublicId,
                        oldPhotoUrl);
                }
            }
            catch (Exception ex)
            {
                // Don't fail the whole update — but the old file becomes orphaned.
                // Log with high severity + exception trace so this can be detected and cleaned up later.
                _logger.LogError(ex,
                    "Cloudinary DeleteAsync THREW during Gallery update. The old file is now ORPHANED. " +
                    "DB update will continue, but please clean up manually. " +
                    "galleryId={GalleryId}, oldPublicId={OldPublicId}, oldUrl={OldUrl}, newPublicId={NewPublicId}",
                    gallery.GalleryId,
                    oldPublicId,
                    oldPhotoUrl,
                    request.PublicId);
            }
        }

        if (request.Title != null) gallery.Title = request.Title;
        if (request.PhotoUrl != null) gallery.PhotoUrl = request.PhotoUrl;
        if (request.ThumbnailUrl != null) gallery.ThumbnailUrl = request.ThumbnailUrl;
        if (request.Category != null) gallery.Category = request.Category;
        if (request.Tags != null) gallery.Tags = request.Tags;
        if (request.OrganizationId.HasValue) gallery.OrganizationId = request.OrganizationId;
        gallery.DisplayOrder = request.DisplayOrder;
        gallery.IsFeatured = request.IsFeatured;

        // Update Cloudinary metadata if file was replaced
        if (request.ReplacingFile)
        {
            if (!string.IsNullOrEmpty(request.PublicId)) gallery.PublicId = request.PublicId;
            if (!string.IsNullOrEmpty(request.OriginalFileName)) gallery.OriginalFileName = request.OriginalFileName;
            if (request.FileSizeBytes.HasValue) gallery.FileSizeBytes = request.FileSizeBytes;
            if (!string.IsNullOrEmpty(request.ContentType)) gallery.ContentType = request.ContentType;
        }

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
            UploadedAt = gallery.UploadedAt,
            PublicId = gallery.PublicId,
            OriginalFileName = gallery.OriginalFileName,
            FileSizeBytes = gallery.FileSizeBytes,
            ContentType = gallery.ContentType
        };
    }
}
