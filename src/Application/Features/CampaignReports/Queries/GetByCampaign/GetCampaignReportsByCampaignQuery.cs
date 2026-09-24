using GiveAID.Application.Features.CampaignReports.DTOs;
using MediatR;

namespace GiveAID.Application.Features.CampaignReports.Queries.GetByCampaign;

/// <summary>
/// Query to get campaign reports by campaign ID.
/// </summary>
public class GetCampaignReportsByCampaignQuery : IRequest<IEnumerable<CampaignReportDto>>
{
    public int CampaignId { get; set; }
}
