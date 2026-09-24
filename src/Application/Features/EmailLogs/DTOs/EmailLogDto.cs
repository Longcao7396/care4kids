namespace GiveAID.Application.Features.EmailLogs.DTOs;

/// <summary>
/// DTO for email log data.
/// </summary>
public class EmailLogDto
{
    public int EmailLogId { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int? RelatedId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
