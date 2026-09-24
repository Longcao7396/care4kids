namespace GiveAID.Application.Common.Interfaces;

/// <summary>
/// Interface for logging emails to the database.
/// Implementation provided by Infrastructure layer.
/// </summary>
public interface IEmailLogService
{
    /// <summary>
    /// Logs an email to the database.
    /// </summary>
    Task LogEmailAsync(string to, string subject, string body, bool success, string? errorMessage = null, CancellationToken cancellationToken = default);
}
