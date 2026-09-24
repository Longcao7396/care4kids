using GiveAID.Application.Features.Campaigns.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Campaigns.Queries.GetFeatured;

/// <summary>
/// Query to get featured campaigns.
/// </summary>
public class GetFeaturedCampaignsQuery : IRequest<IEnumerable<CampaignDto>>
{
    public int Limit { get; set; } = 6;
}
