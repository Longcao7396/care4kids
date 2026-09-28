using GiveAID.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GiveAID.Application.Features.Gallery.Commands.Delete;

/// <summary>
/// Handler for DeleteGalleryCommand.
/// Deletes the gallery item AND the associated Cloudinary file (if any) to avoid orphaned files in storage.
/// </summary>
public class DeleteGalleryCommandHandler : IRequestHandler<DeleteGalleryCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IImageStorageService _imageStorage;
    private readonly ILogger<DeleteGalleryCommandHandler> _logger;

    public DeleteGalleryCommandHandler(
        IApplicationDbContext context,
        IImageStorageService imageStorage,
        ILogger<DeleteGalleryCommandHandler> logger)
    {
        _context = context;
        _imageStorage = imageStorage;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteGalleryCommand request, CancellationToken cancellationToken)
    {
        var gallery = await _context.Gallery.FindAsync(new object[] { request.GalleryId }, cancellationToken);

        if (gallery == null)
        {
            throw new InvalidOperationException($"Gallery item with ID {request.GalleryId} not found.");
        }

        // ===== Delete the file from Cloudinary first =====
        // Capture publicId before removing entity (in case there's a logging/recovery step).
        var publicId = gallery.PublicId;
        var photoUrl = gallery.PhotoUrl;
        var galleryId = gallery.GalleryId;

        if (!string.IsNullOrEmpty(publicId))
        {
            try
            {
                var deleted = await _imageStorage.DeleteAsync(publicId, cancellationToken);
                if (deleted)
                {
                    _logger.LogInformation(
                        "Deleted image from Cloudinary on Gallery delete: galleryId={GalleryId}, publicId={PublicId}, url={Url}",
                        galleryId,
                        publicId,
                        photoUrl);
                }
                else
                {
                    // Cloudinary says file already gone (likely manually removed). DB delete proceeds.
                    _logger.LogWarning(
                        "Cloudinary DeleteAsync returned false during Gallery delete — file may already be absent. " +
                        "Manual cleanup may be needed. galleryId={GalleryId}, publicId={PublicId}, url={Url}",
                        galleryId,
                        publicId,
                        photoUrl);
                }
            }
            catch (Exception ex)
            {
                // Continue with DB delete — orphaned file in Cloudinary is better than stale DB row.
                // Log with high severity + exception trace so this can be detected and cleaned up later.
                _logger.LogError(ex,
                    "Cloudinary DeleteAsync THREW during Gallery delete. The file is now ORPHANED. " +
                    "DB row will still be removed, but please clean up manually. " +
                    "galleryId={GalleryId}, publicId={PublicId}, url={Url}",
                    galleryId,
                    publicId,
                    photoUrl);
            }
        }
        else
        {
            _logger.LogDebug(
                "Gallery delete: no PublicId to clean up on Cloudinary. galleryId={GalleryId}",
                galleryId);
        }

        gallery.IsDeleted = true;
        gallery.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
