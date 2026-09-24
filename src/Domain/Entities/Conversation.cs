using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class Conversation : BaseEntity
{
    [Key]
    public int ConversationId { get; set; }

    public int? UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ConversationType { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Open";

    [MaxLength(20)]
    public string Priority { get; set; } = "Normal";

    public int? AssignedTo { get; set; }
    public DateTime? ClosedAt { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    public virtual ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
}
