namespace GiveAID.Application.Common.Interfaces;

/// <summary>
/// Interface for cloud image storage operations.
/// Implementation: CloudinaryImageStorageService (Infrastructure layer).
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Uploads an image file to cloud storage.
    /// </summary>
    /// <param name="file">The image file stream.</param>
    /// <param name="fileName">Original file name (used for extension detection).</param>
    /// <param name="contentType">MIME type (e.g. image/jpeg).</param>
    /// <param name="folder">Optional sub-folder within the configured upload root.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Upload result with URL, public ID, file size, and content type.</returns>
    Task<ImageUploadResult> UploadAsync(
        Stream file,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an image from cloud storage using its public ID.
    /// </summary>
    /// <param name="publicId">The cloud storage public ID of the image.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted successfully.</returns>
    Task<bool> DeleteAsync(string publicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a thumbnail URL for an existing image using on-the-fly transformations.
    /// </summary>
    /// <param name="originalUrl">The full URL of the original image.</param>
    /// <param name="width">Target width in pixels.</param>
    /// <param name="height">Target height in pixels.</param>
    /// <param name="crop">Crop mode (default: "fill").</param>
    /// <returns>Transformed thumbnail URL, or the original URL if transformation is not applicable.</returns>
    string GenerateThumbnailUrl(string originalUrl, int width = 300, int height = 300, string crop = "fill");
}

/// <summary>
/// Result of a successful image upload.
/// </summary>
public record ImageUploadResult(
    string Url,
    string PublicId,
    long FileSizeBytes,
    string ContentType);
