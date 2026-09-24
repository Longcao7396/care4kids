using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class ConversationMessage : BaseEntity
{
    [Key]
    public int MessageId { get; set; }

    [Required]
    public int ConversationId { get; set; }

    [Required]
    public int SenderId { get; set; }

    [Required]
    public string MessageText { get; set; } = string.Empty;

    public bool IsInternalNote { get; set; }
    public string? Attachments { get; set; }
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("ConversationId")]
    public virtual Conversation? Conversation { get; set; }
}
