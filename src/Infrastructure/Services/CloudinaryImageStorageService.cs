using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GiveAID.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// Alias our own record to avoid collision with CloudinaryDotNet.Actions.ImageUploadResult
using ImageUploadResult = GiveAID.Application.Common.Interfaces.ImageUploadResult;
using CloudinaryUploadResult = CloudinaryDotNet.Actions.ImageUploadResult;

namespace GiveAID.Infrastructure.Services;

/// <summary>
/// Cloudinary implementation of IImageStorageService.
/// Falls back to a placeholder URL when Cloudinary is not configured (dev without keys).
/// </summary>
public class CloudinaryImageStorageService : IImageStorageService
{
    private readonly CloudinaryDotNet.Cloudinary? _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryImageStorageService> _logger;

    // Fallback placeholder for dev without Cloudinary credentials
    private const string PlaceholderUrl = "https://via.placeholder.com/400x300.png?text=No+Image";

    public CloudinaryImageStorageService(
        IOptions<CloudinarySettings> settings,
        ILogger<CloudinaryImageStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (_settings.IsConfigured)
        {
            var account = new Account(
                _settings.CloudName,
                _settings.ApiKey,
                _settings.ApiSecret);

            _cloudinary = new CloudinaryDotNet.Cloudinary(account);

            _logger.LogInformation(
                "CloudinaryImageStorageService initialized with cloud: {CloudName}, folder: {Folder}",
                _settings.CloudName,
                _settings.UploadFolder);
        }
        else
        {
            _logger.LogWarning(
                "Cloudinary is not configured (CloudName, ApiKey, or ApiSecret are missing). " +
                "Image uploads will return placeholder URLs. " +
                "Set Cloudinary__CloudName, Cloudinary__ApiKey, Cloudinary__ApiSecret environment variables.");
        }
    }

    /// <inheritdoc />
    public async Task<ImageUploadResult> UploadAsync(
        Stream file,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        // Graceful fallback: if Cloudinary is not configured, return a placeholder
        if (_cloudinary == null)
        {
            _logger.LogWarning(
                "Cloudinary not configured — returning placeholder URL for {FileName}",
                fileName);

            return new Application.Common.Interfaces.ImageUploadResult(
                Url: PlaceholderUrl,
                PublicId: string.Empty,
                FileSizeBytes: 0,
                ContentType: contentType);
        }

        var fullFolder = string.IsNullOrEmpty(folder)
            ? _settings.UploadFolder
            : $"{_settings.UploadFolder}/{folder.TrimStart('/')}";

        // Reset stream position in case it was already read
        if (file.CanSeek)
            file.Position = 0;

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, file),
            Folder = fullFolder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false,
            // Store metadata
            EagerTransforms = new List<Transformation> { new Transformation().Width(1600).Height(1600).Crop("limit").Quality("auto") },
            EagerAsync = false,
            // Allowed formats
            AllowedFormats = new[] { "jpg", "jpeg", "png", "webp" },
        };

        try
        {
            var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError(
                    "Cloudinary upload failed with status {StatusCode}: {Error}",
                    result.StatusCode,
                    result.Error?.Message);

                throw new InvalidOperationException(
                    $"Image upload failed: {result.Error?.Message ?? "Unknown error"}");
            }

            _logger.LogInformation(
                "Image uploaded to Cloudinary: publicId={PublicId}, url={Url}",
                result.PublicId,
                result.SecureUrl);

            return new Application.Common.Interfaces.ImageUploadResult(
                Url: result.SecureUrl.ToString(),
                PublicId: result.PublicId,
                FileSizeBytes: result.Bytes,
                ContentType: contentType);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Cloudinary upload threw an exception for {FileName}", fileName);
            throw new InvalidOperationException($"Image upload failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (_cloudinary == null)
        {
            _logger.LogWarning(
                "Cloudinary not configured — delete skipped for publicId: {PublicId}",
                publicId);
            return false;
        }

        if (string.IsNullOrWhiteSpace(publicId))
        {
            _logger.LogDebug("DeleteAsync called with empty publicId — skipping");
            return false;
        }

        try
        {
            var deleteParams = new DeletionParams(publicId);
            var result = _cloudinary.Destroy(deleteParams);

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Image deleted from Cloudinary: publicId={PublicId}", publicId);
                return true;
            }

            _logger.LogWarning(
                "Cloudinary delete returned non-OK status {StatusCode} for publicId={PublicId}",
                result.StatusCode,
                publicId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cloudinary delete failed for publicId={PublicId}", publicId);
            return false;
        }
    }

    /// <inheritdoc />
    public string GenerateThumbnailUrl(string originalUrl, int width = 300, int height = 300, string crop = "fill")
    {
        if (string.IsNullOrWhiteSpace(originalUrl))
            return PlaceholderUrl;

        // Only transform Cloudinary URLs
        if (originalUrl.Contains("res.cloudinary.com"))
        {
            var transformation = $"w_{width},h_{height},c_{crop},q_auto,f_auto";

            // Cloudinary URL format: .../{publicId}.{ext}
            // Insert transformation after /image/upload/
            if (originalUrl.Contains("/image/upload/"))
            {
                return originalUrl.Replace("/image/upload/", $"/image/upload/{transformation}/");
            }

            // Video or other resource types
            if (originalUrl.Contains("/video/upload/"))
            {
                return originalUrl.Replace("/video/upload/", $"/video/upload/{transformation}/");
            }
        }

        // For non-Cloudinary URLs (external image hosts like Unsplash),
        // return as-is — they handle their own resizing
        return originalUrl;
    }
}
