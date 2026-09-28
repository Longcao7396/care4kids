namespace GiveAID.Infrastructure.Services;

/// <summary>
/// Configuration for Cloudinary image storage.
/// </summary>
public class CloudinarySettings
{
    public const string SectionName = "Cloudinary";

    /// <summary>
    /// Cloud name from Cloudinary dashboard.
    /// </summary>
    public string CloudName { get; set; } = string.Empty;

    /// <summary>
    /// API Key from Cloudinary dashboard.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// API Secret from Cloudinary dashboard.
    /// </summary>
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// Base folder for all uploads within the Cloudinary account.
    /// </summary>
    public string UploadFolder { get; set; } = "giveaid";

    /// <summary>
    /// Whether Cloudinary is configured and active.
    /// Returns true only when CloudName, ApiKey, and ApiSecret are all non-empty.
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(CloudName) &&
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(ApiSecret);
}
