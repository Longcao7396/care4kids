using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.CampaignReports.Commands.Delete;

/// <summary>
/// Handler for DeleteCampaignReportCommand.
/// Performs a hard delete on CampaignReport (no IsDeleted/DeletedAt fields exist).
/// </summary>
public class DeleteCampaignReportCommandHandler : IRequestHandler<DeleteCampaignReportCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCampaignReportCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCampaignReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _context.CampaignReports.FindAsync(
            new object[] { request.ReportId }, cancellationToken);

        if (report == null)
        {
            return false;
        }

        _context.CampaignReports.Remove(report);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
