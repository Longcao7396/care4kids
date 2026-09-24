using GiveAID.Application.Features.Campaigns.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Campaigns.Queries.GetById;

/// <summary>
/// Query to get a campaign by ID.
/// </summary>
public class GetCampaignByIdQuery : IRequest<CampaignDto>
{
    public int CampaignId { get; set; }
}
