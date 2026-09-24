using GiveAID.Application.Features.Donations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Donations.Queries.GetAll;

/// <summary>
/// Query to get all donations (admin).
/// </summary>
public class GetAllDonationsQuery : IRequest<IEnumerable<DonationDto>>
{
    public string? Status { get; set; }
    public int? UserId { get; set; }
    public int? CampaignId { get; set; }
    public int? CauseId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
