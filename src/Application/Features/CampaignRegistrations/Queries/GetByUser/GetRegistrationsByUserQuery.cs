using GiveAID.Application.Features.CampaignRegistrations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.CampaignRegistrations.Queries.GetByUser;

/// <summary>
/// Query to get registrations by user ID.
/// </summary>
public class GetRegistrationsByUserQuery : IRequest<IEnumerable<RegistrationDto>>
{
    public int UserId { get; set; }
}
