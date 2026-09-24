using GiveAID.Application.Features.CampaignReports.DTOs;
using MediatR;

namespace GiveAID.Application.Features.CampaignReports.Queries.GetAll;

/// <summary>
/// Query to get all campaign reports.
/// </summary>
public class GetAllCampaignReportsQuery : IRequest<IEnumerable<CampaignReportDto>>
{
    public bool PublishedOnly { get; set; } = false;
}
