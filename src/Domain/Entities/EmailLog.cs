using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class EmailLog : BaseEntity
{
    [Key]
    public int EmailLogId { get; set; }

    [Required]
    [MaxLength(150)]
    public string ToEmail { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Subject { get; set; } = string.Empty;

    public string? Body { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }

    [MaxLength(50)]
    public string? EmailType { get; set; }
}
