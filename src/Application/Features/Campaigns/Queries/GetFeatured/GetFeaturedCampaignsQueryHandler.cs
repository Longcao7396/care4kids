using GiveAID.Application.Features.Campaigns.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Campaigns.Queries.GetFeatured;

/// <summary>
/// Handler for GetFeaturedCampaignsQuery.
/// </summary>
public class GetFeaturedCampaignsQueryHandler : IRequestHandler<GetFeaturedCampaignsQuery, IEnumerable<CampaignDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFeaturedCampaignsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CampaignDto>> Handle(GetFeaturedCampaignsQuery request, CancellationToken cancellationToken)
    {
        var campaigns = await _context.Campaigns
            .Include(c => c.Cause)
            .Include(c => c.Organization)
            .Where(c => c.IsFeatured && c.Status == "Active")
            .OrderByDescending(c => c.DisplayOrder)
            .ThenByDescending(c => c.CreatedAt)
            .Take(request.Limit)
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
            MaxParticipants = c.MaxParticipants,
            TargetBeneficiaries = c.TargetBeneficiaries,
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
