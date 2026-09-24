namespace GiveAID.Application.Features.CampaignReports.DTOs;

/// <summary>
/// DTO for campaign report data.
/// </summary>
public class CampaignReportDto
{
    public int ReportId { get; set; }
    public int CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public decimal TotalReceived { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal RemainingAmount { get; set; }
    public int? BeneficiariesReached { get; set; }
    public string? ReportTitle { get; set; }
    public string? ReportContent { get; set; }
    public string? ExpenseBreakdown { get; set; }
    public string? Photos { get; set; }
    public string? Documents { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedDate { get; set; }
    public int? PublishedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
