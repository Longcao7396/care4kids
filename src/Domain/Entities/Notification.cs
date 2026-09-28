using System.ComponentModel.DataAnnotations;
using GiveAID.Domain.Entities;

namespace GiveAID.Domain.Entities;

public class Notification : BaseEntity
{
    [Key]
    public int NotificationId { get; set; }
    
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty; // "Donation", "Registration", "ContactReply", etc.
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RelatedEntityType { get; set; } // "Campaign", "Programme", "ContactMessage"
    public int? RelatedEntityId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
