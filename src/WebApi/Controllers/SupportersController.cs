using GiveAID.Application.Features.Organizations.Queries.GetAll;
using GiveAID.Application.Features.Organizations.Queries.GetFeatured;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Legacy /alias controller for /api/v1/supporters — backed by the same
/// Organizations table as /api/v1/organizations. Kept because the frontend
/// supporter page calls this URL.
/// </summary>
[ApiController]
[Route("api/v1/supporters")]
public class SupportersController : ControllerBase
{
    private readonly ISender _mediator;

    public SupportersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = true,
        [FromQuery] string? type = null)
    {
        var items = await _mediator.Send(new GetAllOrganizationsQuery
        {
            ActiveOnly = activeOnly,
            Type = type
        });
        return Ok(new { success = true, message = "OK", data = items });
    }

    [HttpGet("stats")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var all = await _mediator.Send(new GetAllOrganizationsQuery { ActiveOnly = true });
        var list = all.ToList();
        var stats = new
        {
            total = list.Count,
            supporters = list.Count(o => o.OrganizationType == "Supporter"),
            partners = list.Count(o => o.OrganizationType == "Partner"),
            ngos = list.Count(o => o.OrganizationType == "NGO"),
            totalContribution = list.Sum(o => o.ContributionAmount ?? 0)
        };
        return Ok(new { success = true, message = "OK", data = stats });
    }

    [HttpGet("featured")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatured([FromQuery] int count = 6)
    {
        var items = await _mediator.Send(new GetFeaturedOrganizationsQuery { Limit = count });
        return Ok(new { success = true, message = "OK", data = items });
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var items = await _mediator.Send(new GetAllOrganizationsQuery { ActiveOnly = false });
        var dto = items.FirstOrDefault(o => o.OrganizationId == id);
        if (dto == null)
        {
            return NotFound(new { success = false, message = $"Supporter {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "OK", data = dto });
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status501NotImplemented)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public IActionResult Create([FromBody] object request)
        => StatusCode(501, new { success = false, message = "Not implemented", data = (object?)null });

    [HttpPut("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status501NotImplemented)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public IActionResult Update(int id, [FromBody] object request)
        => StatusCode(501, new { success = false, message = "Not implemented", data = (object?)null });

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status501NotImplemented)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public IActionResult Delete(int id)
        => StatusCode(501, new { success = false, message = "Not implemented", data = (object?)null });
}
