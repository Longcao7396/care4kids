using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class Cause : BaseEntity
{
    [Key]
    public int CauseId { get; set; }

    [MaxLength(20)]
    public string? CauseCode { get; set; }

    [Required]
    [MaxLength(100)]
    public string CauseName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    public decimal TargetAmount { get; set; }
    public decimal RaisedAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public int? ParentCauseId { get; set; }

    // Navigation properties
    [ForeignKey("ParentCauseId")]
    public virtual Cause? ParentCause { get; set; }

    public virtual ICollection<Cause> SubCauses { get; set; } = new List<Cause>();
    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
