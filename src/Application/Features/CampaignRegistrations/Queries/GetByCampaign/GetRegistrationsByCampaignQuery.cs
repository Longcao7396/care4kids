using GiveAID.Application.Features.CampaignRegistrations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.CampaignRegistrations.Queries.GetByCampaign;

/// <summary>
/// Query to get registrations by campaign ID.
/// </summary>
public class GetRegistrationsByCampaignQuery : IRequest<IEnumerable<RegistrationDto>>
{
    public int CampaignId { get; set; }
}
