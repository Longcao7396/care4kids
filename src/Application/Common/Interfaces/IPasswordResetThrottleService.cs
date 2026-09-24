namespace GiveAID.Application.Common.Interfaces;

/// <summary>
/// Service for rate limiting password reset requests with exponential backoff.
/// Prevents abuse by limiting requests per email and per IP with increasing
/// wait times for repeated failures.
/// </summary>
public interface IPasswordResetThrottleService
{
    /// <summary>
    /// Checks if the given email/IP combination is currently rate limited.
    /// </summary>
    /// <param name="email">The email requesting password reset.</param>
    /// <param name="ipAddress">The client IP address.</param>
    /// <returns>A result containing whether throttled and the retry-after time.</returns>
    PasswordResetThrottleResult CheckThrottle(string email, string ipAddress);

    /// <summary>
    /// Records a password reset request attempt for tracking purposes.
    /// </summary>
    void RecordRequest(string email, string ipAddress);
}

/// <summary>
/// Result of checking password reset throttle status.
/// </summary>
public class PasswordResetThrottleResult
{
    public bool IsThrottled { get; set; }
    public int RetryAfterSeconds { get; set; }
    public int AttemptCount { get; set; }
    public string? Message { get; set; }
}
