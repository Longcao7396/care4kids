using MediatR;

namespace GiveAID.Application.Features.Campaigns.Commands.Delete;

/// <summary>
/// Handler for DeleteCampaignCommand.
/// </summary>
public class DeleteCampaignCommandHandler : IRequestHandler<DeleteCampaignCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCampaignCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _context.Campaigns.FindAsync(new object[] { request.CampaignId }, cancellationToken);

        if (campaign == null)
        {
            throw new InvalidOperationException($"Campaign with ID {request.CampaignId} not found.");
        }

        _context.Campaigns.Remove(campaign);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
