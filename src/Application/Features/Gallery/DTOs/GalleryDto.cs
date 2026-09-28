namespace GiveAID.Application.Features.Gallery.DTOs;

/// <summary>
/// DTO for gallery data.
/// </summary>
public class GalleryDto
{
    public int GalleryId { get; set; }
    public string? Title { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; } // Deprecated: use Thumbnail computed property instead. Will be removed after migration.
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime UploadedAt { get; set; }

    // Cloudinary metadata — populated from upload result
    public string? PublicId { get; set; }
    public string? OriginalFileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? ContentType { get; set; }

    // NEW: Computed properties that frontend expects (item.url, item.thumbnail)
    // Url: backward-compatible alias for PhotoUrl
    public string Url => PhotoUrl;

    // Thumbnail: generates Cloudinary on-the-fly thumbnail URL from PhotoUrl.
    // Pattern: w_300,h_300,c_fill,q_auto,f_auto — square crop, 300px, auto quality, auto format.
    // Falls back to PhotoUrl if PhotoUrl is not a Cloudinary URL.
    // After migration drops ThumbnailUrl column, this replaces it entirely.
    // (Logic kept in sync with IImageStorageService.GenerateThumbnailUrl)
    public string? Thumbnail
    {
        get
        {
            if (string.IsNullOrEmpty(PhotoUrl)) return null;

            // If Cloudinary URL, insert transformation after /image/upload/ or /video/upload/
            if (PhotoUrl.Contains("res.cloudinary.com"))
            {
                const string transform = "w_300,h_300,c_fill,q_auto,f_auto/";

                if (PhotoUrl.Contains("/image/upload/"))
                {
                    return PhotoUrl.Replace("/image/upload/", "/image/upload/" + transform);
                }

                if (PhotoUrl.Contains("/video/upload/"))
                {
                    return PhotoUrl.Replace("/video/upload/", "/video/upload/" + transform);
                }
            }

            // Fallback: use the original URL (e.g., external image host like Unsplash)
            return PhotoUrl;
        }
    }
}
