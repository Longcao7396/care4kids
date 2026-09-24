using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for career applications.
/// </summary>
[ApiController]
[Route("api/v1/career-applications")]
public class CareerApplicationsController : ControllerBase
{
    private readonly ISender _mediator;

    public CareerApplicationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Submit a career application (public).
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Submit([FromBody] object request)
    {
        return Ok(new { success = true, message = "Application submitted successfully", data = (object?)null });
    }

    /// <summary>
    /// Get all applications (Admin only).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(new { success = true, message = "OK", data = new { items = Array.Empty<object>(), page, pageSize } });
    }
}
