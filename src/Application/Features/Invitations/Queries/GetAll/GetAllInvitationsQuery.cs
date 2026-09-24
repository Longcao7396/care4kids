using GiveAID.Application.Features.Invitations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Invitations.Queries.GetAll;

/// <summary>
/// Query to get all invitations.
/// </summary>
public class GetAllInvitationsQuery : IRequest<IEnumerable<InvitationDto>>
{
    public string? Status { get; set; }
    public int? InviterUserId { get; set; }
}
