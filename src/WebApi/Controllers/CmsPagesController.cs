using GiveAID.Application.Features.CmsPages.Commands.Create;
using GiveAID.Application.Features.CmsPages.Commands.Update;
using GiveAID.Application.Features.CmsPages.DTOs;
using GiveAID.Application.Features.CmsPages.Queries.GetAll;
using GiveAID.Application.Features.CmsPages.Queries.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for CMS-editable page content. Public endpoints allow
/// key-based or slug-based lookup so frontend AboutPage / ContactPage can
/// render admin-edited text when present, and fall back to hardcoded when
/// the row is missing.
/// </summary>
[ApiController]
[Route("api/v1/cms")]
public class CmsPagesController : ControllerBase
{
    private readonly ISender _mediator;

    public CmsPagesController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all CMS pages. Pass ?keys=key1,key2 to filter by PageKey.
    /// </summary>
    [HttpGet("pages")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPages([FromQuery] string? keys = null)
    {
        var items = (await _mediator.Send(new GetAllCmsPagesQuery { ActiveOnly = false })).ToList();

        if (!string.IsNullOrWhiteSpace(keys))
        {
            var filter = keys
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(k => k.ToLowerInvariant())
                .ToHashSet();
            items = items.Where(p => filter.Contains(p.PageKey.ToLowerInvariant())).ToList();
        }

        return Ok(new { success = true, message = "OK", data = items });
    }

    /// <summary>
    /// Get a CMS page by PageKey or PageSlug. Tries slug first; falls back to
    /// matching by PageKey so admin can store pages with either identifier.
    /// </summary>
    [HttpGet("pages/{keyOrSlug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByKeyOrSlug(string keyOrSlug)
    {
        // 1. Try by slug — use null check instead of catching InvalidOperationException
        var bySlug = await _mediator.Send(new GetCmsPageBySlugQuery { Slug = keyOrSlug });
        if (bySlug != null)
        {
            return Ok(new { success = true, message = "OK", data = bySlug });
        }

        // 2. Fallback: match by PageKey (case-insensitive)
        var all = await _mediator.Send(new GetAllCmsPagesQuery { ActiveOnly = false });
        var byKey = all.FirstOrDefault(p =>
            string.Equals(p.PageKey, keyOrSlug, StringComparison.OrdinalIgnoreCase));
        if (byKey != null)
        {
            return Ok(new { success = true, message = "OK", data = byKey });
        }

        return NotFound(new { success = false, message = $"CMS page '{keyOrSlug}' not found", data = (object?)null });
    }

    [HttpPost("pages")]
    [Authorize(Roles = "Admin,ContentManager")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateCmsPageCommand command)
    {
        var dto = await _mediator.Send(command);
        return Ok(new { success = true, message = "Page created", data = dto });
    }

    [HttpPut("pages/{id:int}")]
    [Authorize(Roles = "Admin,ContentManager")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCmsPageCommand command)
    {
        command.PageId = id;
        var dto = await _mediator.Send(command);
        return Ok(new { success = true, message = "Page updated", data = dto });
    }

    [HttpDelete("pages/{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status501NotImplemented)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public IActionResult Delete(int id)
    {
        // Delete command not yet implemented in Application layer
        return StatusCode(501, new { success = false, message = "Delete not yet implemented", data = (object?)null });
    }
}
