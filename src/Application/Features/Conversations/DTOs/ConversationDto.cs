namespace GiveAID.Application.Features.Conversations.DTOs;

/// <summary>
/// DTO for conversation data.
/// </summary>
public class ConversationDto
{
    public int ConversationId { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? ConversationType { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public List<MessageDto>? Messages { get; set; }
}

/// <summary>
/// DTO for conversation message.
/// </summary>
public class MessageDto
{
    public int MessageId { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string? SenderName { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public bool IsInternalNote { get; set; }
    public DateTime CreatedAt { get; set; }
}
