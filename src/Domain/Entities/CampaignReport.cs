using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAID.Domain.Entities;

public class CampaignReport : BaseEntity
{
    [Key]
    public int ReportId { get; set; }

    [Required]
    public int CampaignId { get; set; }

    [Required]
    public decimal TotalReceived { get; set; }

    [Required]
    public decimal TotalSpent { get; set; }

    public int? BeneficiariesReached { get; set; }

    [MaxLength(200)]
    public string? ReportTitle { get; set; }

    public string? ReportContent { get; set; }
    public string? ExpenseBreakdown { get; set; }
    public string? Photos { get; set; }
    public string? Documents { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedDate { get; set; }
    public int? PublishedBy { get; set; }

    // Navigation properties
    [ForeignKey("CampaignId")]
    public virtual Campaign? Campaign { get; set; }
}
