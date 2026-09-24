using GiveAID.Application.Features.Donations.Queries.GetAll;
using GiveAID.Application.Features.Statistics.Queries.GetDashboard;
using GiveAID.Application.Features.Statistics.Queries.GetOverview;
using GiveAID.Application.Features.Statistics.Queries.GetRecentDonations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for admin dashboard statistics.
/// </summary>
[ApiController]
[Route("api/v1/admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminDashboardController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get dashboard statistics.
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var stats = await _mediator.Send(new GetDashboardStatsQuery());
        return Ok(new { success = true, message = "OK", data = stats });
    }

    /// <summary>
    /// Get overview statistics (public).
    /// </summary>
    [HttpGet("overview")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOverview()
    {
        var stats = await _mediator.Send(new GetOverviewStatsQuery());
        return Ok(new { success = true, message = "OK", data = stats });
    }

    /// <summary>
    /// Get recent donations.
    /// </summary>
    [HttpGet("recent-donations")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRecentDonations([FromQuery] int count = 20)
    {
        var items = await _mediator.Send(new GetRecentDonationsQuery { Limit = count });
        return Ok(new { success = true, message = "OK", data = items });
    }
}
