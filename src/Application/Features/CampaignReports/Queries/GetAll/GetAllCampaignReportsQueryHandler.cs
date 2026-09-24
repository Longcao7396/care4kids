using GiveAID.Application.Features.CampaignReports.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.CampaignReports.Queries.GetAll;

/// <summary>
/// Handler for GetAllCampaignReportsQuery.
/// </summary>
public class GetAllCampaignReportsQueryHandler : IRequestHandler<GetAllCampaignReportsQuery, IEnumerable<CampaignReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCampaignReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CampaignReportDto>> Handle(GetAllCampaignReportsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CampaignReports
            .Include(r => r.Campaign)
            .AsQueryable();

        if (request.PublishedOnly)
        {
            query = query.Where(r => r.IsPublished);
        }

        var reports = await query
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
