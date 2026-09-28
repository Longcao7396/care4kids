using GiveAID.Application.Features.Campaigns.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Campaigns.Queries.GetAll;

/// <summary>
/// Handler for GetAllCampaignsQuery.
/// </summary>
public class GetAllCampaignsQueryHandler : IRequestHandler<GetAllCampaignsQuery, PagedCampaignsResult>
{
    private readonly IApplicationDbContext _context;

    public GetAllCampaignsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedCampaignsResult> Handle(GetAllCampaignsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Campaigns
            .Include(c => c.Cause)
            .Include(c => c.Organization)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(c => c.Status == request.Status);
        }

        if (request.CauseId.HasValue)
        {
            query = query.Where(c => c.CauseId == request.CauseId.Value);
        }

        // M-05: Filter to events-only campaigns when requested
        if (request.EventsOnly)
        {
            query = query.Where(c => c.RegistrationRequired);
        }

        // Bug #1 fix: free-text search on campaign name/description (case-insensitive).
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(c =>
                EF.Functions.Like(c.CampaignName, $"%{term}%") ||
                (c.Description != null && EF.Functions.Like(c.Description, $"%{term}%")));
        }

        // Bug #6 fix: count the true total (pre-pagination) instead of the page's item count.
        var totalCount = await query.CountAsync(cancellationToken);

        var campaigns = await query
            .OrderByDescending(c => c.IsFeatured)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = campaigns.Select(c => new CampaignDto
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
            ExpectedBudget = c.ExpectedBudget,
            ActualBudget = c.ActualBudget,
            Description = c.Description,
            GoalAmount = c.GoalAmount,
            RaisedAmount = c.RaisedAmount,
            PercentageReached = c.GoalAmount > 0 ? Math.Min((c.RaisedAmount / c.GoalAmount) * 100, 100) : 0,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            DaysRemaining = c.EndDate.HasValue ? Math.Max((c.EndDate.Value - DateTime.UtcNow).Days, 0) : null,
            ImageUrl = c.ImageUrl,
            BeneficiariesCount = c.BeneficiariesCount,
            Location = c.Location,
            Status = c.Status,
            IsFeatured = c.IsFeatured,
            DisplayOrder = c.DisplayOrder,
            CreatedBy = c.CreatedBy,
            CreatedAt = c.CreatedAt,
            DonorCount = c.Donations?.Count(d => d.PaymentStatus == "Completed") ?? 0
        });

        return new PagedCampaignsResult
        {
            Items = items,
            TotalCount = totalCount
        };
    }
}
