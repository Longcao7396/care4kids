using GiveAID.Application.Features.TeamMembers.DTOs;
using MediatR;

namespace GiveAID.Application.Features.TeamMembers.Queries.GetFeatured;

/// <summary>
/// Query to get featured team members.
/// </summary>
public class GetFeaturedTeamMembersQuery : IRequest<IEnumerable<TeamMemberDto>>
{
    public int Limit { get; set; } = 6;
}
