using MediatR;

namespace GiveAID.Application.Features.TeamMembers.Commands.Delete;

/// <summary>
/// Command to delete a team member.
/// </summary>
public class DeleteTeamMemberCommand : IRequest<bool>
{
    public int TeamMemberId { get; set; }
}
