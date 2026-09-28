using System;

namespace GiveAID.Domain.Entities;

/// <summary>
/// Audit log entry tracking who changed what, when, and how.
/// Captures create, update, and delete operations on all entities.
/// </summary>
public class AuditLog
{
    public int AuditLogId { get; set; }

    /// <summary>
    /// User ID who performed the action (from JWT claims or system).
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Action type: Create, Update, Delete, Login, Logout, etc.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Entity type name (e.g., "Campaign", "Donation", "User").
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Primary key value of the affected entity (stored as string).
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// JSON snapshot of old values (for Update/Delete operations).
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// JSON snapshot of new values (for Create/Update operations).
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// UTC timestamp when the action occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IP address of the client (if available).
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string (browser/app identifier).
    /// </summary>
    public string? UserAgent { get; set; }
}
