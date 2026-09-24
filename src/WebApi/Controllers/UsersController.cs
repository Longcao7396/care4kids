using System.Security.Claims;
using GiveAID.Application.Features.Auth.Queries.GetCurrentUser;
using GiveAID.Application.Features.Users.Commands.UpdateMyProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for user management.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ISender _mediator;

    public UsersController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get current user profile.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { success = false, message = "Invalid token", data = (object?)null });
        }

        var user = await _mediator.Send(new GetCurrentUserQuery { UserId = userId });
        return Ok(new { success = true, message = "OK", data = user });
    }

    /// <summary>
    /// Update current user profile.
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateMyProfileRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { success = false, message = "Invalid token", data = (object?)null });
        }

        var dto = await _mediator.Send(new UpdateMyProfileCommand
        {
            UserId = userId,
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email
        });
        return Ok(new { success = true, message = "Profile updated", data = dto });
    }
}

/// <summary>
/// Request body for PUT /users/me.
/// </summary>
public class UpdateMyProfileRequest
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
