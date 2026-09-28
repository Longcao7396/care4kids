using GiveAID.Application.Features.AdminRegistrations.Commands.ApproveRegistration;
using GiveAID.Application.Features.AdminRegistrations.Commands.RejectRegistration;
using GiveAID.Application.Features.AdminRegistrations.Queries.GetAllRegistrations;
using GiveAID.Application.Features.AdminRegistrations.Queries.GetRegistrationStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Admin-only endpoints for managing user registrations against campaigns
/// and programmes. Powers the /admin/registrations page.
/// </summary>
[ApiController]
[Route("api/v1/admin/registrations")]
[Authorize(Policy = "RequireAdmin")]
public class AdminRegistrationsController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminRegistrationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// List every registration with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await _mediator.Send(new GetAllRegistrationsQuery
        {
            Status = status,
            Type = type,
            Search = search,
            Page = page,
            PageSize = pageSize,
        });

        return Ok(new
        {
            success = true,
            message = "OK",
            data = result,
        });
    }

    /// <summary>
    /// Aggregate counts for the admin dashboard chips.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _mediator.Send(new GetRegistrationStatsQuery());
        return Ok(new { success = true, message = "OK", data = stats });
    }

    /// <summary>
    /// Approve a registration. Body: { registrationType: "Campaign" | "Programme", registrationId: number }
    /// </summary>
    [HttpPost("approve")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Approve([FromBody] RegistrationActionRequest body)
    {
        if (body == null || body.RegistrationId <= 0)
        {
            return BadRequest(new { success = false, message = "registrationId is required.", data = (object?)null });
        }

        var dto = await _mediator.Send(new ApproveRegistrationCommand
        {
            RegistrationId = body.RegistrationId,
            RegistrationType = string.IsNullOrWhiteSpace(body.RegistrationType) ? "Campaign" : body.RegistrationType,
            ReviewedBy = body.ReviewedBy ?? User.Identity?.Name,
        });
        return Ok(new { success = true, message = "Registration approved", data = dto });
    }

    /// <summary>
    /// Reject a registration. Body: { registrationType, registrationId, reason? }
    /// </summary>
    [HttpPost("reject")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Reject([FromBody] RegistrationActionRequest body)
    {
        if (body == null || body.RegistrationId <= 0)
        {
            return BadRequest(new { success = false, message = "registrationId is required.", data = (object?)null });
        }

        try
        {
            var dto = await _mediator.Send(new RejectRegistrationCommand
            {
                RegistrationId = body.RegistrationId,
                RegistrationType = string.IsNullOrWhiteSpace(body.RegistrationType) ? "Campaign" : body.RegistrationType,
                ReviewedBy = body.ReviewedBy ?? User.Identity?.Name,
                Reason = body.Reason,
            });
            return Ok(new { success = true, message = "Registration rejected", data = dto });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message, data = (object?)null });
        }
    }
}

/// <summary>
/// Request body for POST /admin/registrations/approve and /reject.
/// </summary>
public class RegistrationActionRequest
{
    public string RegistrationType { get; set; } = "Campaign";
    public int RegistrationId { get; set; }
    public string? ReviewedBy { get; set; }
    public string? Reason { get; set; }
}