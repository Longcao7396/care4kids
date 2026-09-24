using GiveAID.Application.Features.CampaignReports.DTOs;
using MediatR;

namespace GiveAID.Application.Features.CampaignReports.Commands.Update;

/// <summary>
/// Command to update a campaign report.
/// </summary>
public class UpdateCampaignReportCommand : IRequest<CampaignReportDto>
{
    public int ReportId { get; set; }
    public decimal? TotalReceived { get; set; }
    public decimal? TotalSpent { get; set; }
    public int? BeneficiariesReached { get; set; }
    public string? ReportTitle { get; set; }
    public string? ReportContent { get; set; }
    public string? ExpenseBreakdown { get; set; }
    public string? Photos { get; set; }
    public string? Documents { get; set; }
}
