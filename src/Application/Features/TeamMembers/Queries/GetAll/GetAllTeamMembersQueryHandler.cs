using GiveAID.Application.Features.TeamMembers.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.TeamMembers.Queries.GetAll;

/// <summary>
/// Handler for GetAllTeamMembersQuery.
/// </summary>
public class GetAllTeamMembersQueryHandler : IRequestHandler<GetAllTeamMembersQuery, IEnumerable<TeamMemberDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllTeamMembersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TeamMemberDto>> Handle(GetAllTeamMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TeamMembers.AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        var members = await query
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.FullName)
            .ToListAsync(cancellationToken);

        return members.Select(MapToDto);
    }

    private TeamMemberDto MapToDto(Domain.Entities.TeamMember t)
    {
        return new TeamMemberDto
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
        };
    }
}
