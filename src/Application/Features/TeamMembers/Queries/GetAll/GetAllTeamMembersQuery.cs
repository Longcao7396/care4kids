using GiveAID.Application.Features.TeamMembers.DTOs;
using MediatR;

namespace GiveAID.Application.Features.TeamMembers.Queries.GetAll;

/// <summary>
/// Query to get all team members.
/// </summary>
public class GetAllTeamMembersQuery : IRequest<IEnumerable<TeamMemberDto>>
{
    public bool ActiveOnly { get; set; } = true;
}
