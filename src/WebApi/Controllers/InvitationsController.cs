using System.Security.Claims;
using GiveAID.Application.Features.Invitations.Commands.CancelInvitation;
using GiveAID.Application.Features.Invitations.Commands.CreateInvitation;
using GiveAID.Application.Features.Invitations.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for managing invitations.
/// </summary>
[ApiController]
[Route("api/v1/invitations")]
[Authorize]
public class InvitationsController : ControllerBase
{
    private readonly ISender _mediator;

    public InvitationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create/send an invitation.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InvitationRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { success = false, message = "Invalid token", data = (object?)null });
        }

        var dto = await _mediator.Send(new CreateInvitationCommand
        {
            InviterUserId = userId,
            InviteeName = request.InviteeName,
            InviteeEmail = request.InviteeEmail,
            PersonalMessage = request.PersonalMessage
        });
        return Ok(new { success = true, message = "Invitation sent", data = dto });
    }

    /// <summary>
    /// Get my invitations.
    /// </summary>
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { success = false, message = "Invalid token", data = (object?)null });
        }

        var items = await _mediator.Send(new GetAllInvitationsQuery { InviterUserId = userId });
        return Ok(new { success = true, message = "OK", data = items });
    }

    /// <summary>
    /// Get all invitations (Admin only).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 30, [FromQuery] string? status = null)
    {
        var items = await _mediator.Send(new GetAllInvitationsQuery { Status = status });
        return Ok(new { success = true, message = "OK", data = new { items, page, pageSize, totalCount = items.Count() } });
    }

    /// <summary>
    /// Get invitation statistics.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var items = await _mediator.Send(new GetAllInvitationsQuery());
        var list = items.ToList();
        var stats = new
        {
            total = list.Count,
            pending = list.Count(i => i.Status == "Pending"),
            accepted = list.Count(i => i.Status == "Accepted"),
            expired = list.Count(i => i.Status == "Expired")
        };
        return Ok(new { success = true, message = "OK", data = stats });
    }

    /// <summary>
    /// Cancel an invitation.
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var ok = await _mediator.Send(new CancelInvitationCommand { InvitationId = id });
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Invitation {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "Invitation cancelled", data = new { invitationId = id } });
    }
}

/// <summary>
/// Request body for POST /invitations.
/// </summary>
public class InvitationRequest
{
    public string InviteeName { get; set; } = string.Empty;
    public string InviteeEmail { get; set; } = string.Empty;
    public string? PersonalMessage { get; set; }
}
