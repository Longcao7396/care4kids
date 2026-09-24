using GiveAID.Application.Features.Invitations.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.Invitations.Commands.CreateInvitation;

/// <summary>
/// Command to create an invitation.
/// </summary>
public class CreateInvitationCommand : IRequest<InvitationDto>
{
    public int InviterUserId { get; set; }
    public string InviteeName { get; set; } = string.Empty;
    public string InviteeEmail { get; set; } = string.Empty;
    public string? PersonalMessage { get; set; }
}
