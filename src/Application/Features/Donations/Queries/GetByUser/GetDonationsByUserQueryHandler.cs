using GiveAID.Application.Features.Donations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Donations.Queries.GetByUser;

/// <summary>
/// Handler for GetDonationsByUserQuery.
/// </summary>
public class GetDonationsByUserQueryHandler : IRequestHandler<GetDonationsByUserQuery, IEnumerable<DonationSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDonationsByUserQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DonationSummaryDto>> Handle(GetDonationsByUserQuery request, CancellationToken cancellationToken)
    {
        var donations = await _context.Donations
            .Include(d => d.Cause)
            .Include(d => d.Campaign)
            .Where(d => d.UserId == request.UserId)
            .OrderByDescending(d => d.DonationDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return donations.Select(d => new DonationSummaryDto
        {
            DonationId = d.DonationId,
            Amount = d.Amount,
            CauseName = d.Cause?.CauseName,
            CampaignName = d.Campaign?.CampaignName,
            PaymentStatus = d.PaymentStatus,
            DonationDate = d.DonationDate,
            IsAnonymous = d.IsAnonymous
        });
    }
}
