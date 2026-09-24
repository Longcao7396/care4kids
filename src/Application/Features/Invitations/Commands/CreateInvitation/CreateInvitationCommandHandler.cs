using GiveAID.Application.Features.Invitations.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.Invitations.Commands.CreateInvitation;

/// <summary>
/// Handler for CreateInvitationCommand.
/// </summary>
public class CreateInvitationCommandHandler : IRequestHandler<CreateInvitationCommand, InvitationDto>
{
    private readonly IApplicationDbContext _context;

    public CreateInvitationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InvitationDto> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = new Invitation
        {
            InviterUserId = request.InviterUserId,
            InviteeName = request.InviteeName,
            InviteeEmail = request.InviteeEmail,
            PersonalMessage = request.PersonalMessage,
            Status = "Pending",
            InvitationToken = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync(cancellationToken);

        var inviter = await _context.Users.FindAsync(new object[] { request.InviterUserId }, cancellationToken);

        return new InvitationDto
        {
            InvitationId = invitation.InvitationId,
            InviterUserId = invitation.InviterUserId,
            InviterName = inviter?.FullName,
            InviteeName = invitation.InviteeName,
            InviteeEmail = invitation.InviteeEmail,
            PersonalMessage = invitation.PersonalMessage,
            Status = invitation.Status,
            InvitationToken = invitation.InvitationToken,
            CreatedAt = invitation.CreatedAt
        };
    }
}
