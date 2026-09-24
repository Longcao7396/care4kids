using MediatR;

namespace GiveAID.Application.Features.CampaignReports.Commands.Publish;

/// <summary>
/// Handler for PublishCampaignReportCommand.
/// </summary>
public class PublishCampaignReportCommandHandler : IRequestHandler<PublishCampaignReportCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public PublishCampaignReportCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(PublishCampaignReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _context.CampaignReports.FindAsync(new object[] { request.ReportId }, cancellationToken);

        if (report == null)
        {
            throw new InvalidOperationException($"Campaign report with ID {request.ReportId} not found.");
        }

        report.IsPublished = true;
        report.PublishedDate = DateTime.UtcNow;
        report.PublishedBy = request.PublishedBy;
        report.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
