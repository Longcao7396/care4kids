namespace GiveAID.Application.Common.Interfaces;

/// <summary>
/// Interface for HTML sanitization to prevent XSS attacks.
/// Implementation provided by Infrastructure layer.
/// </summary>
public interface IHtmlSanitizer
{
    /// <summary>
    /// Sanitizes HTML by removing dangerous tags and attributes.
    /// </summary>
    /// <param name="html">The HTML to sanitize.</param>
    /// <returns>Sanitized HTML.</returns>
    string Sanitize(string html);
}
