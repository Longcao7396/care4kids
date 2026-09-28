using System.Security.Claims;
using GiveAID.Application.Features.Donations.Commands.ManualConfirm;
using GiveAID.Application.Features.Donations.DTOs;
using GiveAID.Application.Features.Donations.Queries.GetAll;
using GiveAID.Application.Features.Donations.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for admin payment management.
/// </summary>
[ApiController]
[Route("api/v1/admin/payments")]
[Authorize(Policy = "RequireAdmin")]
public class AdminPaymentsController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminPaymentsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all donations/payments.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] int? causeId = null,
        [FromQuery] int? campaignId = null)
    {
        var items = await _mediator.Send(new GetAllDonationsQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            CauseId = causeId,
            CampaignId = campaignId
        });
        return Ok(new { success = true, message = "OK", data = new { items, page, pageSize } });
    }

    /// <summary>
    /// Get donation by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var dto = await _mediator.Send(new GetDonationByIdQuery { DonationId = id });
            return Ok(new { success = true, message = "OK", data = dto });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { success = false, message = $"Donation {id} not found", data = (object?)null });
        }
    }

    /// <summary>
    /// Manual confirm a donation (admin override).
    /// </summary>
    [HttpPost("{id:int}/confirm")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ManualConfirm(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { success = false, message = "Invalid token", data = (object?)null });
        }

        var ok = await _mediator.Send(new ManualConfirmCommand
        {
            DonationId = id,
            ConfirmedBy = userId
        });
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Donation {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "Donation manually confirmed", data = new { donationId = id } });
    }

    /// <summary>
    /// Refund a donation.
    /// </summary>
    [HttpPost("{id:int}/refund")]
    [ProducesResponseType(typeof(object), StatusCodes.Status501NotImplemented)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public IActionResult Refund(int id)
    {
        // Refund processing requires payment gateway integration — stub pending
        return StatusCode(501, new { success = false, message = "Refund not yet implemented — requires gateway integration", data = new { donationId = id } });
    }
}
