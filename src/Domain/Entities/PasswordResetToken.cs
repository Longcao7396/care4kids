using System;

namespace GiveAID.Domain.Entities;

/// <summary>
/// Stores one-time password reset tokens with expiration.
/// Used by the forgot-password flow to ensure tokens are:
/// - Cryptographically secure (Guid.NewGuid() without hyphens)
/// - Single-use (UsedAt set after successful reset)
/// - Time-limited (ExpiresAt, default 1 hour)
/// - Tied to a specific user account
/// </summary>
public class PasswordResetToken
{
    public int Id { get; set; }

    /// <summary>
    /// The user requesting the password reset.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The reset token (cryptographically random, stored plain, validated by equality).
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// When this token expires. After this time, the token is rejected.
    /// Default: 1 hour from creation.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Set when the token is successfully used to reset the password.
    /// A token can only be used once.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// IP address of the requester (for audit purposes).
    /// </summary>
    public string? RequestIpAddress { get; set; }

    /// <summary>
    /// User agent of the requester (for audit purposes).
    /// </summary>
    public string? RequestUserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual User? User { get; set; }

    /// <summary>
    /// Checks whether this token is still valid (not expired and not already used).
    /// </summary>
    public bool IsValid => UsedAt == null && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// Marks this token as used (one-time use).
    /// </summary>
    public void MarkAsUsed()
    {
        UsedAt = DateTime.UtcNow;
    }
}
