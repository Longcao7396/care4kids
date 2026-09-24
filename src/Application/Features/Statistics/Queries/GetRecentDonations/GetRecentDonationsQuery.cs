using GiveAID.Application.Features.Statistics.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Statistics.Queries.GetRecentDonations;

/// <summary>
/// Query to get recent donations.
/// </summary>
public class GetRecentDonationsQuery : IRequest<IEnumerable<RecentDonationDto>>
{
    public int Limit { get; set; } = 10;
}
