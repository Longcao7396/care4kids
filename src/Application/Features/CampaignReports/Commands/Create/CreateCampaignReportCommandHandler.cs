using GiveAID.Application.Features.CampaignReports.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.CampaignReports.Commands.Create;

/// <summary>
/// Handler for CreateCampaignReportCommand.
/// </summary>
public class CreateCampaignReportCommandHandler : IRequestHandler<CreateCampaignReportCommand, CampaignReportDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCampaignReportCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CampaignReportDto> Handle(CreateCampaignReportCommand request, CancellationToken cancellationToken)
    {
        var report = new CampaignReport
        {
            CampaignId = request.CampaignId,
            TotalReceived = request.TotalReceived,
            TotalSpent = request.TotalSpent,
            BeneficiariesReached = request.BeneficiariesReached,
            ReportTitle = request.ReportTitle,
            ReportContent = request.ReportContent,
            ExpenseBreakdown = request.ExpenseBreakdown,
            Photos = request.Photos,
            Documents = request.Documents,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.CampaignReports.Add(report);
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
            CreatedAt = report.CreatedAt
        };
    }
}
