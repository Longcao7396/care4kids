using GiveAID.Application.Features.Campaigns.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Campaigns.Queries.GetById;

/// <summary>
/// Handler for GetCampaignByIdQuery.
/// </summary>
public class GetCampaignByIdQueryHandler : IRequestHandler<GetCampaignByIdQuery, CampaignDto>
{
    private readonly IApplicationDbContext _context;

    public GetCampaignByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CampaignDto> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var campaign = await _context.Campaigns
            .Include(c => c.Cause)
            .Include(c => c.Organization)
            .Include(c => c.Donations)
            .FirstOrDefaultAsync(c => c.CampaignId == request.CampaignId, cancellationToken);

        if (campaign == null)
        {
            throw new InvalidOperationException($"Campaign with ID {request.CampaignId} not found.");
        }

        return new CampaignDto
        {
            CampaignId = campaign.CampaignId,
            CauseId = campaign.CauseId,
            CauseName = campaign.Cause?.CauseName,
            OrganizationId = campaign.OrganizationId,
            OrganizationName = campaign.Organization?.OrganizationName,
            CampaignName = campaign.CampaignName,
            CampaignCode = campaign.CampaignCode,
            ProgrammeType = campaign.ProgrammeType,
            RegistrationRequired = campaign.RegistrationRequired,
            MaxParticipants = campaign.MaxParticipants,
            TargetBeneficiaries = campaign.TargetBeneficiaries,
            ExpectedBudget = campaign.ExpectedBudget,
            ActualBudget = campaign.ActualBudget,
            Description = campaign.Description,
            GoalAmount = campaign.GoalAmount,
            RaisedAmount = campaign.RaisedAmount,
            PercentageReached = campaign.GoalAmount > 0 ? Math.Min((campaign.RaisedAmount / campaign.GoalAmount) * 100, 100) : 0,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            DaysRemaining = campaign.EndDate.HasValue ? Math.Max((campaign.EndDate.Value - DateTime.UtcNow).Days, 0) : null,
            ImageUrl = campaign.ImageUrl,
            BeneficiariesCount = campaign.BeneficiariesCount,
            Location = campaign.Location,
            Status = campaign.Status,
            IsFeatured = campaign.IsFeatured,
            DisplayOrder = campaign.DisplayOrder,
            CreatedBy = campaign.CreatedBy,
            CreatedAt = campaign.CreatedAt,
            DonorCount = campaign.Donations?.Count(d => d.PaymentStatus == "Completed") ?? 0
        };
    }
}
