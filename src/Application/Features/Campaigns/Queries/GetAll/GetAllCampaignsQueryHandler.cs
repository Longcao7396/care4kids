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
        // M-05 FIX: Use projection with Select() to avoid N+1 queries
        // Old approach: Include() loaded full entities, then .Select() in memory (51 queries)
        // New approach: Project directly in SQL (1-2 queries)
        var query = _context.Campaigns.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(c => c.Status == request.Status);
        }

        if (request.CauseId.HasValue)
        {
            query = query.Where(c => c.CauseId == request.CauseId.Value);
        }

        // Filter to events-only campaigns when requested
        if (request.EventsOnly)
        {
            query = query.Where(c => c.RegistrationRequired);
        }

        // Free-text search on campaign name/description (case-insensitive)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(c =>
                EF.Functions.Like(c.CampaignName, $"%{term}%") ||
                (c.Description != null && EF.Functions.Like(c.Description, $"%{term}%")));
        }

        // Count the true total (pre-pagination)
        var totalCount = await query.CountAsync(cancellationToken);

        // Project directly to DTO in SQL (avoid loading full entities)
        var items = await query
            .OrderByDescending(c => c.IsFeatured)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CampaignDto
            {
                CampaignId = c.CampaignId,
                CauseId = c.CauseId,
                CauseName = c.Cause != null ? c.Cause.CauseName : null,
                OrganizationId = c.OrganizationId,
                OrganizationName = c.Organization != null ? c.Organization.OrganizationName : null,
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
                DonorCount = c.Donations.Count(d => d.PaymentStatus == "Completed")
            })
            .ToListAsync(cancellationToken);

        return new PagedCampaignsResult
        {
            Items = items,
            TotalCount = totalCount
        };
    }
}
