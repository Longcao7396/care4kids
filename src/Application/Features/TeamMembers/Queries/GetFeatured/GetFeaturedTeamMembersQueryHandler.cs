using GiveAID.Application.Features.TeamMembers.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.TeamMembers.Queries.GetFeatured;

/// <summary>
/// Handler for GetFeaturedTeamMembersQuery.
/// </summary>
public class GetFeaturedTeamMembersQueryHandler : IRequestHandler<GetFeaturedTeamMembersQuery, IEnumerable<TeamMemberDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFeaturedTeamMembersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TeamMemberDto>> Handle(GetFeaturedTeamMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _context.TeamMembers
            .Where(t => t.IsFeatured && t.IsActive)
            .OrderBy(t => t.DisplayOrder)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return members.Select(t => new TeamMemberDto
        {
            TeamMemberId = t.TeamMemberId,
            FullName = t.FullName,
            RoleTitle = t.RoleTitle,
            Department = t.Department,
            Bio = t.Bio,
            PhotoUrl = t.PhotoUrl,
            Email = t.Email,
            LinkedInUrl = t.LinkedInUrl,
            TwitterUrl = t.TwitterUrl,
            FacebookUrl = t.FacebookUrl,
            IsActive = t.IsActive,
            IsFeatured = t.IsFeatured,
            DisplayOrder = t.DisplayOrder,
            JoinedDate = t.JoinedDate
        });
    }
}
