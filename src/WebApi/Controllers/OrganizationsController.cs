using GiveAID.Application.Features.Organizations.DTOs;
using GiveAID.Application.Features.Organizations.Queries.GetAll;
using GiveAID.Application.Features.Organizations.Queries.GetFeatured;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for organizations/partners.
/// </summary>
[ApiController]
[Route("api/v1/organizations")]
public class OrganizationsController : ControllerBase
{
    private readonly ISender _mediator;

    public OrganizationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all organizations. Optional filters: ?activeOnly=true&type=NGO
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
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

    /// <summary>
    /// Get featured organizations.
    /// </summary>
    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeatured([FromQuery] int count = 6)
    {
        var items = await _mediator.Send(new GetFeaturedOrganizationsQuery { Limit = count });
        return Ok(new { success = true, message = "OK", data = items });
    }

    /// <summary>
    /// Get stats: counts by type, total contribution.
    /// </summary>
    [HttpGet("stats")]
    [AllowAnonymous]
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
            corporates = list.Count(o => o.OrganizationType == "Corporate"),
            governments = list.Count(o => o.OrganizationType == "Government"),
            totalContribution = list.Sum(o => o.ContributionAmount ?? 0)
        };
        return Ok(new { success = true, message = "OK", data = stats });
    }

    /// <summary>
    /// Get organization by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var items = await _mediator.Send(new GetAllOrganizationsQuery { ActiveOnly = false });
        var dto = items.FirstOrDefault(o => o.OrganizationId == id);
        if (dto == null)
        {
            return NotFound(new { success = false, message = $"Organization {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "OK", data = dto });
    }

    /// <summary>
    /// Create an organization (Admin only).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult Create([FromBody] object request)
    {
        // Write path is intentionally a stub here — POST goes through admin
        // management screens which have their own controllers; keep this
        // public unauthenticated endpoint minimal until admin endpoint is wired.
        return StatusCode(501, new { success = false, message = "Create not yet implemented", data = (object?)null });
    }

    /// <summary>
    /// Update an organization (Admin only).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Update(int id, [FromBody] object request)
    {
        return StatusCode(501, new { success = false, message = "Update not yet implemented", data = (object?)null });
    }

    /// <summary>
    /// Delete an organization (Admin only).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        return StatusCode(501, new { success = false, message = "Delete not yet implemented", data = (object?)null });
    }
}
