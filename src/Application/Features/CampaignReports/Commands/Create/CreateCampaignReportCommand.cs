using GiveAID.Application.Features.CampaignReports.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.CampaignReports.Commands.Create;

/// <summary>
/// Command to create a campaign report.
/// </summary>
public class CreateCampaignReportCommand : IRequest<CampaignReportDto>
{
    public int CampaignId { get; set; }
    public decimal TotalReceived { get; set; }
    public decimal TotalSpent { get; set; }
    public int? BeneficiariesReached { get; set; }
    public string? ReportTitle { get; set; }
    public string? ReportContent { get; set; }
    public string? ExpenseBreakdown { get; set; }
    public string? Photos { get; set; }
    public string? Documents { get; set; }
}
