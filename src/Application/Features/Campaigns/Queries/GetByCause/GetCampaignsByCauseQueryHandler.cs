using GiveAID.Application.Features.Campaigns.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Campaigns.Queries.GetByCause;

/// <summary>
/// Handler for GetCampaignsByCauseQuery.
/// </summary>
public class GetCampaignsByCauseQueryHandler : IRequestHandler<GetCampaignsByCauseQuery, IEnumerable<CampaignDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCampaignsByCauseQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CampaignDto>> Handle(GetCampaignsByCauseQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Campaigns
            .Include(c => c.Cause)
            .Include(c => c.Organization)
            .Where(c => c.CauseId == request.CauseId);

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(c => c.Status == request.Status);
        }

        var campaigns = await query
            .OrderByDescending(c => c.IsFeatured)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return campaigns.Select(c => new CampaignDto
        {
            CampaignId = c.CampaignId,
            CauseId = c.CauseId,
            CauseName = c.Cause?.CauseName,
            OrganizationId = c.OrganizationId,
            OrganizationName = c.Organization?.OrganizationName,
            CampaignName = c.CampaignName,
            CampaignCode = c.CampaignCode,
            ProgrammeType = c.ProgrammeType,
            RegistrationRequired = c.RegistrationRequired,
            Description = c.Description,
            GoalAmount = c.GoalAmount,
            RaisedAmount = c.RaisedAmount,
            PercentageReached = c.GoalAmount > 0 ? Math.Min((c.RaisedAmount / c.GoalAmount) * 100, 100) : 0,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            DaysRemaining = c.EndDate.HasValue ? Math.Max((c.EndDate.Value - DateTime.UtcNow).Days, 0) : null,
            ImageUrl = c.ImageUrl,
            Location = c.Location,
            Status = c.Status,
            IsFeatured = c.IsFeatured,
            DisplayOrder = c.DisplayOrder,
            CreatedAt = c.CreatedAt,
            DonorCount = c.Donations?.Count(d => d.PaymentStatus == "Completed") ?? 0
        });
    }
}
