using System.Security.Claims;
using GiveAID.Application.Features.Conversations.Commands.AddMessage;
using GiveAID.Application.Features.Conversations.Commands.Assign;
using GiveAID.Application.Features.Conversations.Commands.Close;
using GiveAID.Application.Features.Conversations.Commands.Create;
using GiveAID.Application.Features.Conversations.Queries.GetAll;
using GiveAID.Application.Features.Conversations.Queries.GetById;
using GiveAID.Application.Features.Conversations.Queries.GetByUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for conversations/support tickets.
/// </summary>
[ApiController]
[Route("api/v1/conversations")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly ISender _mediator;

    public ConversationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new conversation.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConversationRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);

        var dto = await _mediator.Send(new CreateConversationCommand
        {
            UserId = userId > 0 ? userId : null,
            Subject = request.Subject,
            ConversationType = request.ConversationType,
            Priority = request.Priority,
            InitialMessage = request.InitialMessage
        });
        return Ok(new { success = true, message = "Conversation created", data = dto });
    }

    /// <summary>
    /// Add a message to a conversation.
    /// </summary>
    [HttpPost("{id:int}/messages")]
    public async Task<IActionResult> AddMessage(int id, [FromBody] AddMessageRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { success = false, message = "Invalid token", data = (object?)null });
        }

        var dto = await _mediator.Send(new AddMessageCommand
        {
            ConversationId = id,
            SenderId = userId,
            MessageText = request.MessageText,
            IsInternalNote = request.IsInternalNote
        });
        return Ok(new { success = true, message = "Message added", data = dto });
    }

    /// <summary>
    /// Get my conversations.
    /// </summary>
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine([FromQuery] string? status = null)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { success = false, message = "Invalid token", data = (object?)null });
        }

        var items = await _mediator.Send(new GetConversationsByUserQuery { UserId = userId });
        return Ok(new { success = true, message = "OK", data = items });
    }

    /// <summary>
    /// Get conversation by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var dto = await _mediator.Send(new GetConversationByIdQuery { ConversationId = id });
            return Ok(new { success = true, message = "OK", data = dto });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { success = false, message = $"Conversation {id} not found", data = (object?)null });
        }
    }

    /// <summary>
    /// Get all conversations (Admin only).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null)
    {
        var items = await _mediator.Send(new GetAllConversationsQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            Priority = priority
        });
        return Ok(new { success = true, message = "OK", data = new { items, page, pageSize } });
    }

    /// <summary>
    /// Close a conversation.
    /// </summary>
    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(int id)
    {
        var ok = await _mediator.Send(new CloseConversationCommand { ConversationId = id });
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Conversation {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "Conversation closed", data = (object?)null });
    }

    /// <summary>
    /// Assign a conversation to an admin.
    /// </summary>
    [HttpPost("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignConversationRequest request)
    {
        var ok = await _mediator.Send(new AssignConversationCommand
        {
            ConversationId = id,
            AssignedTo = request.AssignedTo
        });
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Conversation {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "Conversation assigned", data = new { conversationId = id, assignedTo = request.AssignedTo } });
    }
}

/// <summary>
/// Request body for POST /conversations.
/// </summary>
public class CreateConversationRequest
{
    public string Subject { get; set; } = string.Empty;
    public string? ConversationType { get; set; }
    public string? Priority { get; set; }
    public string? InitialMessage { get; set; }
}

/// <summary>
/// Request body for POST /conversations/{id}/messages.
/// </summary>
public class AddMessageRequest
{
    public string MessageText { get; set; } = string.Empty;
    public bool IsInternalNote { get; set; }
}

/// <summary>
/// Request body for POST /conversations/{id}/assign.
/// </summary>
public class AssignConversationRequest
{
    public int AssignedTo { get; set; }
}
