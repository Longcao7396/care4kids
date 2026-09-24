using MediatR;

namespace GiveAID.Application.Features.CampaignReports.Commands.Delete;

/// <summary>
/// Command to delete a campaign report.
/// </summary>
public class DeleteCampaignReportCommand : IRequest<bool>
{
    public int ReportId { get; set; }
}
