using GiveAID.Application.Features.TeamMembers.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.TeamMembers.Commands.Create;

/// <summary>
/// Command to create a team member.
/// </summary>
public class CreateTeamMemberCommand : IRequest<TeamMemberDto>
{
    public string FullName { get; set; } = string.Empty;
    public string RoleTitle { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Email { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime? JoinedDate { get; set; }
    public int? CreatedBy { get; set; }
}
