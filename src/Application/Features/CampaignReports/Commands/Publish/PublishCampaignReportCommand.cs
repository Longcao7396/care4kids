using MediatR;

namespace GiveAID.Application.Features.CampaignReports.Commands.Publish;

/// <summary>
/// Command to publish a campaign report.
/// </summary>
public class PublishCampaignReportCommand : IRequest<bool>
{
    public int ReportId { get; set; }
    public int PublishedBy { get; set; }
}
