using GiveAID.Application.Features.Statistics.DTOs;
using GiveAID.Application.Features.Statistics.Queries.GetDashboard;
using GiveAID.Application.Features.Statistics.Queries.GetOverview;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for public statistics.
/// </summary>
[ApiController]
[Route("api/v1/statistics")]
public class StatisticsController : ControllerBase
{
    private readonly ISender _mediator;

    public StatisticsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get overview statistics (public).
    /// </summary>
    [HttpGet("overview")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        var stats = await _mediator.Send(new GetOverviewStatsQuery());
        return Ok(new { success = true, message = "OK", data = stats });
    }

    /// <summary>
    /// Get dashboard statistics (Admin).
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboard()
    {
        var stats = await _mediator.Send(new GetDashboardStatsQuery());
        return Ok(new { success = true, message = "OK", data = stats });
    }
}
