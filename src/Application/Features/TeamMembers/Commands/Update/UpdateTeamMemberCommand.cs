using GiveAID.Application.Features.TeamMembers.DTOs;
using MediatR;

namespace GiveAID.Application.Features.TeamMembers.Commands.Update;

/// <summary>
/// Command to update a team member.
/// </summary>
public class UpdateTeamMemberCommand : IRequest<TeamMemberDto>
{
    public int TeamMemberId { get; set; }
    public string? FullName { get; set; }
    public string? RoleTitle { get; set; }
    public string? Department { get; set; }
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Email { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
    public int? DisplayOrder { get; set; }
    public DateTime? JoinedDate { get; set; }
}
