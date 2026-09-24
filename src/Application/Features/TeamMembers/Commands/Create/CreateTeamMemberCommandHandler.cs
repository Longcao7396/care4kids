using GiveAID.Application.Features.TeamMembers.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.TeamMembers.Commands.Create;

/// <summary>
/// Handler for CreateTeamMemberCommand.
/// </summary>
public class CreateTeamMemberCommandHandler : MediatR.IRequestHandler<CreateTeamMemberCommand, TeamMemberDto>
{
    private readonly IApplicationDbContext _context;

    public CreateTeamMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeamMemberDto> Handle(CreateTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var member = new TeamMember
        {
            FullName = request.FullName,
            RoleTitle = request.RoleTitle,
            Department = request.Department,
            Bio = request.Bio,
            PhotoUrl = request.PhotoUrl,
            Email = request.Email,
            LinkedInUrl = request.LinkedInUrl,
            TwitterUrl = request.TwitterUrl,
            FacebookUrl = request.FacebookUrl,
            IsFeatured = request.IsFeatured,
            DisplayOrder = request.DisplayOrder,
            JoinedDate = request.JoinedDate,
            IsActive = true,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TeamMembers.Add(member);
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
