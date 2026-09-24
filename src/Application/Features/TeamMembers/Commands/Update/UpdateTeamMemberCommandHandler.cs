using GiveAID.Application.Features.TeamMembers.DTOs;
using MediatR;

namespace GiveAID.Application.Features.TeamMembers.Commands.Update;

/// <summary>
/// Handler for UpdateTeamMemberCommand.
/// </summary>
public class UpdateTeamMemberCommandHandler : IRequestHandler<UpdateTeamMemberCommand, TeamMemberDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateTeamMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeamMemberDto> Handle(UpdateTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _context.TeamMembers.FindAsync(new object[] { request.TeamMemberId }, cancellationToken);

        if (member == null)
        {
            throw new InvalidOperationException($"Team member with ID {request.TeamMemberId} not found.");
        }

        if (request.FullName != null) member.FullName = request.FullName;
        if (request.RoleTitle != null) member.RoleTitle = request.RoleTitle;
        if (request.Department != null) member.Department = request.Department;
        if (request.Bio != null) member.Bio = request.Bio;
        if (request.PhotoUrl != null) member.PhotoUrl = request.PhotoUrl;
        if (request.Email != null) member.Email = request.Email;
        if (request.LinkedInUrl != null) member.LinkedInUrl = request.LinkedInUrl;
        if (request.TwitterUrl != null) member.TwitterUrl = request.TwitterUrl;
        if (request.FacebookUrl != null) member.FacebookUrl = request.FacebookUrl;
        if (request.IsActive.HasValue) member.IsActive = request.IsActive.Value;
        if (request.IsFeatured.HasValue) member.IsFeatured = request.IsFeatured.Value;
        if (request.DisplayOrder.HasValue) member.DisplayOrder = request.DisplayOrder.Value;
        if (request.JoinedDate.HasValue) member.JoinedDate = request.JoinedDate;
        member.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new TeamMemberDto
        {
            TeamMemberId = member.TeamMemberId,
            FullName = member.FullName,
            RoleTitle = member.RoleTitle,
            Department = member.Department,
            Bio = member.Bio,
            PhotoUrl = member.PhotoUrl,
            Email = member.Email,
            LinkedInUrl = member.LinkedInUrl,
            TwitterUrl = member.TwitterUrl,
            FacebookUrl = member.FacebookUrl,
            IsActive = member.IsActive,
            IsFeatured = member.IsFeatured,
            DisplayOrder = member.DisplayOrder,
            JoinedDate = member.JoinedDate
        };
    }
}
