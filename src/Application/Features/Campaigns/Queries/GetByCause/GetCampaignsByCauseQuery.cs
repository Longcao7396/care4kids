using GiveAID.Application.Features.Campaigns.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Campaigns.Queries.GetByCause;

/// <summary>
/// Query to get campaigns by cause ID.
/// </summary>
public class GetCampaignsByCauseQuery : IRequest<IEnumerable<CampaignDto>>
{
    public int CauseId { get; set; }
    public string? Status { get; set; }
}
