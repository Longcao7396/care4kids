using GiveAID.Application.Features.Donations.Commands.ManualConfirm;
using GiveAID.Application.Services;

namespace GiveAID.Application.Features.Donations.Commands.ManualConfirm;

/// <summary>
/// Handler for ManualConfirmCommand.
/// </summary>
public class ManualConfirmCommandHandler : IRequestHandler<ManualConfirmCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public ManualConfirmCommandHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<bool> Handle(ManualConfirmCommand request, CancellationToken cancellationToken)
    {
        var donation = await _context.Donations.FindAsync(
            new object[] { request.DonationId }, cancellationToken);

        if (donation == null)
        {
            throw new InvalidOperationException($"Donation with ID {request.DonationId} not found.");
        }

        // M-10: Use domain method to enforce state machine transitions
        // Domain method handles idempotency (returns early if already completed)
        donation.MarkAsCompleted();

        // Update campaign raised amount
        if (donation.CampaignId.HasValue)
        {
            var campaign = await _context.Campaigns.FindAsync(
                new object[] { donation.CampaignId.Value }, cancellationToken);
            if (campaign != null)
            {
                campaign.RaisedAmount += donation.Amount;
            }
        }

        // Update cause raised amount
        var cause = await _context.Causes.FindAsync(
            new object[] { donation.CauseId }, cancellationToken);
        if (cause != null)
        {
            cause.RaisedAmount += donation.Amount;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate statistics cache
        _cacheService.InvalidateStatistics();

        return true;
    }
}
