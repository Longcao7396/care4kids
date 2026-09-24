using GiveAID.Application.Features.Donations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Donations.Queries.GetById;

/// <summary>
/// Query to get a donation by ID.
/// </summary>
public class GetDonationByIdQuery : IRequest<DonationDto>
{
    public int DonationId { get; set; }
}
