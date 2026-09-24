using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class Organization : BaseEntity
{
    [Key]
    public int OrganizationId { get; set; }

    [Required]
    [MaxLength(150)]
    public string OrganizationName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string OrganizationType { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(255)]
    public string? LogoUrl { get; set; }

    [MaxLength(200)]
    public string? WebsiteUrl { get; set; }

    [MaxLength(100)]
    public string? ContactEmail { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(500)]
    public string? Mission { get; set; }

    [MaxLength(500)]
    public string? Vision { get; set; }

    public decimal? ContributionAmount { get; set; }

    [MaxLength(50)]
    public string? ContributionType { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation properties
    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    public virtual ICollection<Programme> Programmes { get; set; } = new List<Programme>();
    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public virtual ICollection<Gallery> GalleryItems { get; set; } = new List<Gallery>();
}
