using GiveAID.Application.Features.Contacts.Commands.Submit;
using GiveAID.Application.Features.Contacts.DTOs;
using GiveAID.Application.Features.Contacts.Queries.GetAll;
using GiveAID.Application.Features.Contacts.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for contact form submissions.
/// </summary>
[ApiController]
[Route("api/v1/contacts")]
public class ContactsController : ControllerBase
{
    private readonly ISender _mediator;

    public ContactsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Submit a contact form (public).
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Submit([FromBody] ContactSubmitDto dto)
    {
        var result = await _mediator.Send(new SubmitContactCommand
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Subject = dto.Subject,
            Message = dto.Message
        });
        return Ok(new
        {
            success = true,
            message = "Your message has been sent. We will get back to you soon.",
            data = result
        });
    }

    /// <summary>
    /// Get all contact submissions (Admin only).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var items = await _mediator.Send(new GetAllContactsQuery
        {
            Page = page,
            PageSize = pageSize
        });
        return Ok(new
        {
            success = true,
            message = "OK",
            data = new { items, page, pageSize, totalCount = items.Count() }
        });
    }

    /// <summary>
    /// Get contact stats (Admin only).
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStats()
    {
        var all = await _mediator.Send(new GetAllContactsQuery { Page = 1, PageSize = 1000 });
        var list = all.ToList();
        var stats = new
        {
            total = list.Count,
            unread = list.Count(c => !c.IsRead),
            read = list.Count(c => c.IsRead),
            replied = list.Count(c => c.RepliedAt.HasValue)
        };
        return Ok(new { success = true, message = "OK", data = stats });
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var dto = await _mediator.Send(new GetContactByIdQuery { ContactId = id });
            return Ok(new { success = true, message = "OK", data = dto });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { success = false, message = $"Contact {id} not found", data = (object?)null });
        }
    }

    [HttpPut("{id:int}/reply")]
    [Authorize(Roles = "Admin")]
    public IActionResult Reply(int id, [FromBody] object request)
        => StatusCode(501, new { success = false, message = "Reply not yet implemented", data = (object?)null });

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
        => StatusCode(501, new { success = false, message = "Delete not yet implemented", data = (object?)null });
}

/// <summary>Public DTO accepted by POST /api/v1/contacts.</summary>
public class ContactSubmitDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
