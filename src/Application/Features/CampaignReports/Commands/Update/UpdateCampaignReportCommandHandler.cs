using GiveAID.Application.Features.CampaignReports.DTOs;
using MediatR;

namespace GiveAID.Application.Features.CampaignReports.Commands.Update;

/// <summary>
/// Handler for UpdateCampaignReportCommand.
/// </summary>
public class UpdateCampaignReportCommandHandler : IRequestHandler<UpdateCampaignReportCommand, CampaignReportDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCampaignReportCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CampaignReportDto> Handle(UpdateCampaignReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _context.CampaignReports.FindAsync(new object[] { request.ReportId }, cancellationToken);

        if (report == null)
        {
            throw new InvalidOperationException($"Campaign report with ID {request.ReportId} not found.");
        }

        if (request.TotalReceived.HasValue) report.TotalReceived = request.TotalReceived.Value;
        if (request.TotalSpent.HasValue) report.TotalSpent = request.TotalSpent.Value;
        if (request.BeneficiariesReached.HasValue) report.BeneficiariesReached = request.BeneficiariesReached;
        if (request.ReportTitle != null) report.ReportTitle = request.ReportTitle;
        if (request.ReportContent != null) report.ReportContent = request.ReportContent;
        if (request.ExpenseBreakdown != null) report.ExpenseBreakdown = request.ExpenseBreakdown;
        if (request.Photos != null) report.Photos = request.Photos;
        if (request.Documents != null) report.Documents = request.Documents;
        report.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new CampaignReportDto
        {
            ReportId = report.ReportId,
            CampaignId = report.CampaignId,
            TotalReceived = report.TotalReceived,
            TotalSpent = report.TotalSpent,
            RemainingAmount = report.TotalReceived - report.TotalSpent,
            BeneficiariesReached = report.BeneficiariesReached,
            ReportTitle = report.ReportTitle,
            ReportContent = report.ReportContent,
            ExpenseBreakdown = report.ExpenseBreakdown,
            Photos = report.Photos,
            Documents = report.Documents,
            IsPublished = report.IsPublished,
            PublishedDate = report.PublishedDate,
            PublishedBy = report.PublishedBy,
            CreatedAt = report.CreatedAt
        };
    }
}
