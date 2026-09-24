using GiveAID.Application.Features.EmailLogs.Commands.RetryEmail;
using GiveAID.Application.Features.EmailLogs.Queries.GetAll;
using GiveAID.Application.Features.EmailLogs.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for admin email logs management.
/// </summary>
[ApiController]
[Route("api/v1/admin/emails")]
[Authorize(Roles = "Admin")]
public class AdminEmailLogsController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminEmailLogsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all email logs.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? category = null)
    {
        var items = await _mediator.Send(new GetAllEmailLogsQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            Category = category
        });
        return Ok(new { success = true, message = "OK", data = new { items, page, pageSize } });
    }

    /// <summary>
    /// Get email log by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var dto = await _mediator.Send(new GetEmailLogByIdQuery { EmailLogId = id });
            return Ok(new { success = true, message = "OK", data = dto });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { success = false, message = $"Email log {id} not found", data = (object?)null });
        }
    }

    /// <summary>
    /// Retry sending a failed email.
    /// </summary>
    [HttpPost("{id:int}/retry")]
    public async Task<IActionResult> Retry(int id)
    {
        var ok = await _mediator.Send(new RetryEmailCommand { EmailLogId = id });
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Email log {id} not found or not retryable", data = (object?)null });
        }
        return Ok(new { success = true, message = "Email resent successfully", data = (object?)null });
    }
}
