namespace GiveAID.Application.Features.Auth.DTOs;

/// <summary>
/// Minimal DTO for the current user's profile returned by /auth/me.
/// 
/// SECURITY FIX #2: Only safe, non-sensitive fields are returned.
/// DO NOT add: password hash, security stamp, phone, address, DOB,
/// internal flags, or any PII that the user didn't explicitly opt to share.
/// </summary>
public class UserProfileDto
{
    /// <summary>
    /// User's unique identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// User's email address (they already know this, no privacy concern).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's display name / full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// User's role for UI routing and authorization checks.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Optional avatar URL for profile display.
    /// </summary>
    public string? AvatarUrl { get; set; }
}
