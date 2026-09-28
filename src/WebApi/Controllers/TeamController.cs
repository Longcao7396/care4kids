using GiveAID.Application.Features.TeamMembers.Commands.Create;
using GiveAID.Application.Features.TeamMembers.Commands.Delete;
using GiveAID.Application.Features.TeamMembers.Commands.Update;
using GiveAID.Application.Features.TeamMembers.DTOs;
using GiveAID.Application.Features.TeamMembers.Queries.GetAll;
using GiveAID.Application.Features.TeamMembers.Queries.GetFeatured;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for team members.
/// </summary>
[ApiController]
[Route("api/v1/team")]
public class TeamController : ControllerBase
{
    private readonly ISender _mediator;

    public TeamController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all team members.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var items = await _mediator.Send(new GetAllTeamMembersQuery { ActiveOnly = activeOnly });
        return Ok(new { success = true, message = "OK", data = items });
    }

    /// <summary>
    /// Get featured team members.
    /// </summary>
    [HttpGet("featured")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatured([FromQuery] int limit = 6)
    {
        var items = await _mediator.Send(new GetFeaturedTeamMembersQuery { Limit = limit });
        return Ok(new { success = true, message = "OK", data = items });
    }

    /// <summary>
    /// Get team member by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var all = await _mediator.Send(new GetAllTeamMembersQuery { ActiveOnly = false });
        var dto = all.FirstOrDefault(t => t.TeamMemberId == id);
        if (dto == null)
        {
            return NotFound(new { success = false, message = $"Team member {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "OK", data = dto });
    }

    /// <summary>
    /// Create a new team member (Admin only).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateTeamMemberRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);

        var command = new CreateTeamMemberCommand
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
            CreatedBy = userId > 0 ? userId : null
        };

        var dto = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = dto.TeamMemberId },
            new { success = true, message = "Team member created", data = dto });
    }

    /// <summary>
    /// Update a team member (Admin only).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTeamMemberRequest request)
    {
        var command = new UpdateTeamMemberCommand
        {
            TeamMemberId = id,
            FullName = request.FullName,
            RoleTitle = request.RoleTitle,
            Department = request.Department,
            Bio = request.Bio,
            PhotoUrl = request.PhotoUrl,
            Email = request.Email,
            LinkedInUrl = request.LinkedInUrl,
            TwitterUrl = request.TwitterUrl,
            FacebookUrl = request.FacebookUrl,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            DisplayOrder = request.DisplayOrder,
            JoinedDate = request.JoinedDate
        };

        var dto = await _mediator.Send(command);
        return Ok(new { success = true, message = "Team member updated", data = dto });
    }

    /// <summary>
    /// Delete a team member (Admin only).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _mediator.Send(new DeleteTeamMemberCommand { TeamMemberId = id });
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Team member {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "Team member deleted", data = (object?)null });
    }
}

/// <summary>
/// Request body for POST /team.
/// </summary>
public class CreateTeamMemberRequest
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
}

/// <summary>
/// Request body for PUT /team/{id}.
/// </summary>
public class UpdateTeamMemberRequest
{
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
