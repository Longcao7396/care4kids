using GiveAID.Application.Features.Users.Commands.DeactivateUser;
using GiveAID.Application.Features.Users.Commands.DeleteUser;
using GiveAID.Application.Features.Users.Commands.UpdateUser;
using GiveAID.Application.Features.Users.DTOs;
using GiveAID.Application.Features.Users.Queries.GetAll;
using GiveAID.Application.Features.Users.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for admin user management.
/// </summary>
[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Policy = "RequireAdmin")]
public class AdminUsersController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminUsersController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all users with pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null)
    {
        var items = await _mediator.Send(new GetAllUsersQuery
        {
            Page = page,
            PageSize = pageSize,
            Role = role,
            IsActive = isActive
        });
        return Ok(new { success = true, message = "OK", data = new { items, page, pageSize } });
    }

    /// <summary>
    /// Get user by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var dto = await _mediator.Send(new GetUserByIdQuery { UserId = id });
            return Ok(new { success = true, message = "OK", data = dto });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { success = false, message = $"User {id} not found", data = (object?)null });
        }
    }

    /// <summary>
    /// Update user (role, profile).
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var dto = await _mediator.Send(new UpdateUserCommand
        {
            UserId = id,
            Username = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            Phone = request.Phone,
            Role = request.Role
        });
        return Ok(new { success = true, message = "User updated", data = dto });
    }

    /// <summary>
    /// Deactivate a user.
    /// </summary>
    [HttpPut("{id:int}/deactivate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Deactivate(int id)
    {
        var ok = await _mediator.Send(new DeactivateUserCommand { UserId = id });
        if (!ok)
        {
            return NotFound(new { success = false, message = $"User {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "User deactivated", data = (object?)null });
    }

    /// <summary>
    /// Delete a user (soft delete).
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _mediator.Send(new DeleteUserCommand { UserId = id });
        if (!ok)
        {
            return NotFound(new { success = false, message = $"User {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "User deleted", data = new { userId = id } });
    }
}

/// <summary>
/// Request body for PUT /admin/users/{id}.
/// </summary>
public class UpdateUserRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
}
