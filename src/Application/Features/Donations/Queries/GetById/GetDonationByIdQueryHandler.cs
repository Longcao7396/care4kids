using GiveAID.Application.Features.Donations.DTOs;
using GiveAID.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Donations.Queries.GetById;

/// <summary>
/// Handler for GetDonationByIdQuery.
/// </summary>
public class GetDonationByIdQueryHandler : IRequestHandler<GetDonationByIdQuery, DonationDto>
{
    private readonly IApplicationDbContext _context;

    public GetDonationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DonationDto> Handle(GetDonationByIdQuery request, CancellationToken cancellationToken)
    {
        var donation = await _context.Donations
            .Include(d => d.User)
            .Include(d => d.Cause)
            .Include(d => d.Campaign)
            .Include(d => d.Organization)
            .FirstOrDefaultAsync(d => d.DonationId == request.DonationId, cancellationToken);

        if (donation == null)
        {
            throw new InvalidOperationException($"Donation with ID {request.DonationId} not found.");
        }

        return new DonationDto
        {
            DonationId = donation.DonationId,
            UserId = donation.UserId,
            UserName = donation.IsAnonymous ? "Anonymous" : donation.User?.FullName,
            CauseId = donation.CauseId,
            CauseName = donation.Cause?.CauseName,
            CampaignId = donation.CampaignId,
            CampaignName = donation.Campaign?.CampaignName,
            OrganizationId = donation.OrganizationId,
            OrganizationName = donation.Organization?.OrganizationName,
            Amount = donation.Amount,
            PaymentMethod = donation.PaymentMethod,
            PaymentStatus = donation.PaymentStatus,
            CardLastFour = donation.CardLastFour,
            CardType = donation.CardType,
            TransactionId = donation.TransactionId,
            Message = donation.Message,
            IsAnonymous = donation.IsAnonymous,
            ReceiptSent = donation.ReceiptSent,
            DonationDate = donation.DonationDate,
            CreatedAt = donation.CreatedAt
        };
    }
}
