using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class Programme : BaseEntity
{
    [Key]
    public int ProgrammeId { get; set; }

    public int? OrganizationId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ProgrammeType { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [MaxLength(255)]
    public string? Location { get; set; }

    public int? TargetBeneficiaries { get; set; }
    public decimal? ExpectedBudget { get; set; }
    public decimal? ActualBudget { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Upcoming";

    public bool IsFeatured { get; set; }
    public bool RegistrationRequired { get; set; } = true;
    public int? MaxParticipants { get; set; }
    public int? CreatedBy { get; set; }

    // Navigation properties
    [ForeignKey("OrganizationId")]
    public virtual Organization? Organization { get; set; }

    public virtual ICollection<ProgrammeRegistration> Registrations { get; set; } = new List<ProgrammeRegistration>();
    public virtual ICollection<ProgrammePhoto> Photos { get; set; } = new List<ProgrammePhoto>();
    public virtual ICollection<Gallery> GalleryItems { get; set; } = new List<Gallery>();
}
