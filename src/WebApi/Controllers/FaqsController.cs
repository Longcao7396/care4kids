using GiveAID.Application.Features.Faqs.Commands.Create;
using GiveAID.Application.Features.Faqs.Commands.Delete;
using GiveAID.Application.Features.Faqs.Commands.Update;
using GiveAID.Application.Features.Faqs.DTOs;
using GiveAID.Application.Features.Faqs.Queries.GetAll;
using GiveAID.Application.Features.Faqs.Queries.GetCategories;
using GiveAID.Application.Features.Faqs.Queries.GetFeatured;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for FAQs used by the public Help Centre page.
/// </summary>
[ApiController]
[Route("api/v1/faqs")]
public class FaqsController : ControllerBase
{
    private readonly ISender _mediator;

    public FaqsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var items = await _mediator.Send(new GetAllFaqsQuery { ActiveOnly = activeOnly });
        return Ok(new { success = true, message = "OK", data = items });
    }

    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeatured([FromQuery] int count = 6)
    {
        var items = await _mediator.Send(new GetFeaturedFaqsQuery { Limit = count });
        return Ok(new { success = true, message = "OK", data = items });
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var items = await _mediator.Send(new GetFaqCategoriesQuery());
        return Ok(new { success = true, message = "OK", data = items });
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var items = await _mediator.Send(new GetAllFaqsQuery { ActiveOnly = false });
        var dto = items.FirstOrDefault(f => f.FaqId == id);
        if (dto == null)
        {
            return NotFound(new { success = false, message = $"FAQ {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "OK", data = dto });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] FaqCreateDto dto)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);
        var command = new CreateFaqCommand
        {
            Question = dto.Question,
            Answer = dto.Answer,
            Category = dto.Category,
            DisplayOrder = dto.DisplayOrder,
            IsFeatured = dto.IsFeatured,
            CreatedBy = userId > 0 ? (int?)userId : null
        };
        var newId = await _mediator.Send(command);
        return Ok(new { success = true, message = "FAQ created", data = new { id = newId } });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] FaqUpdateDto dto)
    {
        await _mediator.Send(new UpdateFaqCommand
        {
            FaqId = id,
            Question = dto.Question,
            Answer = dto.Answer,
            Category = dto.Category,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            IsFeatured = dto.IsFeatured
        });
        return Ok(new { success = true, message = "FAQ updated", data = (object?)null });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteFaqCommand { FaqId = id });
        return Ok(new { success = true, message = "FAQ deleted", data = (object?)null });
    }
}

/// <summary>Public DTO accepted by POST /api/v1/faqs.</summary>
public class FaqCreateDto
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }
}

/// <summary>Public DTO accepted by PUT /api/v1/faqs/{id}.</summary>
public class FaqUpdateDto
{
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public string? Category { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
}
