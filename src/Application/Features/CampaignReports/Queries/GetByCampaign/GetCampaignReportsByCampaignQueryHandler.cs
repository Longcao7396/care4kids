using GiveAID.Application.Features.CampaignReports.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.CampaignReports.Queries.GetByCampaign;

/// <summary>
/// Handler for GetCampaignReportsByCampaignQuery.
/// </summary>
public class GetCampaignReportsByCampaignQueryHandler : IRequestHandler<GetCampaignReportsByCampaignQuery, IEnumerable<CampaignReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCampaignReportsByCampaignQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CampaignReportDto>> Handle(GetCampaignReportsByCampaignQuery request, CancellationToken cancellationToken)
    {
        var reports = await _context.CampaignReports
            .Include(r => r.Campaign)
            .Where(r => r.CampaignId == request.CampaignId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return reports.Select(r => new CampaignReportDto
        {
            ReportId = r.ReportId,
            CampaignId = r.CampaignId,
            CampaignName = r.Campaign?.CampaignName,
            TotalReceived = r.TotalReceived,
            TotalSpent = r.TotalSpent,
            RemainingAmount = r.TotalReceived - r.TotalSpent,
            BeneficiariesReached = r.BeneficiariesReached,
            ReportTitle = r.ReportTitle,
            ReportContent = r.ReportContent,
            ExpenseBreakdown = r.ExpenseBreakdown,
            Photos = r.Photos,
            Documents = r.Documents,
            IsPublished = r.IsPublished,
            PublishedDate = r.PublishedDate,
            PublishedBy = r.PublishedBy,
            CreatedAt = r.CreatedAt
        });
    }
}
