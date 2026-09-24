using MediatR;

namespace GiveAID.Application.Features.Invitations.Commands.CancelInvitation;

/// <summary>
/// Handler for CancelInvitationCommand.
/// </summary>
public class CancelInvitationCommandHandler : IRequestHandler<CancelInvitationCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CancelInvitationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CancelInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _context.Invitations.FindAsync(
            new object[] { request.InvitationId }, cancellationToken);

        if (invitation == null)
        {
            return false;
        }

        invitation.Status = "Cancelled";
        invitation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
