using GiveAID.Application.Features.Donations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Donations.Queries.GetByUser;

/// <summary>
/// Query to get donations by user ID.
/// </summary>
public class GetDonationsByUserQuery : IRequest<IEnumerable<DonationSummaryDto>>
{
    public int UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
