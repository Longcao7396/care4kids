using MediatR;

namespace GiveAID.Application.Features.Invitations.Commands.CancelInvitation;

/// <summary>
/// Command to cancel an invitation.
/// </summary>
public class CancelInvitationCommand : IRequest<bool>
{
    public int InvitationId { get; set; }
}
