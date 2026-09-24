using GiveAID.Application.Features.Statistics.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Statistics.Queries.GetRecentDonations;

/// <summary>
/// Handler for GetRecentDonationsQuery.
/// </summary>
public class GetRecentDonationsQueryHandler : IRequestHandler<GetRecentDonationsQuery, IEnumerable<RecentDonationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRecentDonationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RecentDonationDto>> Handle(GetRecentDonationsQuery request, CancellationToken cancellationToken)
    {
        var donations = await _context.Donations
            .Include(d => d.User)
            .Include(d => d.Campaign)
            .Include(d => d.Cause)
            .Where(d => d.PaymentStatus == "Completed")
            .OrderByDescending(d => d.DonationDate)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return donations.Select(d => new RecentDonationDto
        {
            DonationId = d.DonationId,
            Amount = d.Amount,
            DonorName = d.IsAnonymous ? "Anonymous" : d.User?.FullName,
            CampaignName = d.Campaign?.CampaignName,
            CauseName = d.Cause?.CauseName,
            DonationDate = d.DonationDate,
            IsAnonymous = d.IsAnonymous
        });
    }
}
