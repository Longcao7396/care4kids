using GiveAID.Application.Features.Donations.DTOs;
using GiveAID.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Donations.Queries.GetAll;

/// <summary>
/// Handler for GetAllDonationsQuery.
/// </summary>
public class GetAllDonationsQueryHandler : IRequestHandler<GetAllDonationsQuery, IEnumerable<DonationDto>>
{
    private readonly IApplicationDbContext _context;
    
    // M-06: Configurable page size limits
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public GetAllDonationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DonationDto>> Handle(GetAllDonationsQuery request, CancellationToken cancellationToken)
    {
        // M-06: Enforce page size limits to prevent excessive loads
        var pageSize = request.PageSize;
        if (pageSize <= 0) pageSize = DefaultPageSize;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;
        
        var page = Math.Max(request.Page, 1);
        
        var query = _context.Donations
            .Include(d => d.User)
            .Include(d => d.Cause)
            .Include(d => d.Campaign)
            .Include(d => d.Organization)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(d => d.PaymentStatus == request.Status);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(d => d.UserId == request.UserId.Value);
        }

        if (request.CampaignId.HasValue)
        {
            query = query.Where(d => d.CampaignId == request.CampaignId.Value);
        }

        if (request.CauseId.HasValue)
        {
            query = query.Where(d => d.CauseId == request.CauseId.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(d => d.DonationDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(d => d.DonationDate <= request.ToDate.Value);
        }

        var donations = await query
            .OrderByDescending(d => d.DonationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return donations.Select(d => new DonationDto
        {
            DonationId = d.DonationId,
            UserId = d.UserId,
            UserName = d.IsAnonymous ? "Anonymous" : d.User?.FullName,
            CauseId = d.CauseId,
            CauseName = d.Cause?.CauseName,
            CampaignId = d.CampaignId,
            CampaignName = d.Campaign?.CampaignName,
            OrganizationId = d.OrganizationId,
            OrganizationName = d.Organization?.OrganizationName,
            Amount = d.Amount,
            PaymentMethod = d.PaymentMethod,
            PaymentStatus = d.PaymentStatus,
            CardLastFour = d.CardLastFour,
            CardType = d.CardType,
            TransactionId = d.TransactionId,
            Message = d.Message,
            IsAnonymous = d.IsAnonymous,
            ReceiptSent = d.ReceiptSent,
            DonationDate = d.DonationDate,
            CreatedAt = d.CreatedAt
        });
    }
}
