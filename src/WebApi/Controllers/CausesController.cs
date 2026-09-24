using GiveAID.Application.Features.Causes.Commands;
using GiveAID.Application.Features.Causes.DTOs;
using GiveAID.Application.Features.Causes.Queries.GetAll;
using GiveAID.Application.Features.Causes.Queries.GetById;
using GiveAID.Application.Features.Causes.Queries.GetTree;
using GiveAID.Application.Features.Statistics.Queries.GetCauses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for managing causes (categories of charitable campaigns).
/// </summary>
[ApiController]
[Route("api/v1/causes")]
public class CausesController : ControllerBase
{
    private readonly ISender _mediator;

    public CausesController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all causes (flat list).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var items = await _mediator.Send(new GetAllCausesQuery { ActiveOnly = activeOnly });
        return Ok(new { success = true, message = "OK", data = items });
    }

    /// <summary>
    /// Get causes in tree structure (parent → sub-causes).
    /// </summary>
    [HttpGet("tree")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTree()
    {
        var items = await _mediator.Send(new GetCauseTreeQuery());
        return Ok(new { success = true, message = "OK", data = items });
    }

    /// <summary>
    /// Get cause by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var dto = await _mediator.Send(new GetCauseByIdQuery { CauseId = id });
            return Ok(new { success = true, message = "OK", data = dto });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { success = false, message = $"Cause {id} not found", data = (object?)null });
        }
    }

    /// <summary>
    /// Get cause statistics (MAJ-008).
    /// </summary>
    [HttpGet("stats")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _mediator.Send(new GetCausesStatsQuery());
        return Ok(new { success = true, message = "OK", data = stats });
    }

    /// <summary>
    /// Create a new cause (Admin only).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CauseCreateDto dto)
    {
        var newId = await _mediator.Send(new CreateCauseCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = newId },
            new { success = true, message = "Cause created", data = new { id = newId } });
    }

    /// <summary>
    /// Update a cause (Admin only).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CauseUpdateDto dto)
    {
        await _mediator.Send(new UpdateCauseCommand(id, dto));
        return Ok(new { success = true, message = "Cause updated", data = (object?)null });
    }

    /// <summary>
    /// Delete a cause (Admin only).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteCauseCommand(id));
        return Ok(new { success = true, message = "Cause deleted", data = (object?)null });
    }
}
