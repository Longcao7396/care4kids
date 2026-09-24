namespace GiveAID.Application.Features.Contacts.DTOs;

/// <summary>
/// DTO for contact message data.
/// </summary>
public class ContactDto
{
    public int ContactId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public int? RepliedBy { get; set; }
    public string? ReplyMessage { get; set; }
    public DateTime? RepliedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
