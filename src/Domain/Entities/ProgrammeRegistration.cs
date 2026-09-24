using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class ProgrammeRegistration : BaseEntity
{
    [Key]
    public int RegistrationId { get; set; }

    [Required]
    public int ProgrammeId { get; set; }

    [Required]
    public int UserId { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Registered";

    [MaxLength(500)]
    public string? Notes { get; set; }

    public bool AttendanceConfirmed { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("ProgrammeId")]
    public virtual Programme? Programme { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
