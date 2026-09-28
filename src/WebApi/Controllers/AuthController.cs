using System.Security.Claims;
using GiveAID.Application.Features.Auth.Commands.ForgotPassword;
using GiveAID.Application.Features.Auth.Commands.Login;
using GiveAID.Application.Features.Auth.Commands.Register;
using GiveAID.Application.Features.Auth.Commands.ResetPassword;
using GiveAID.Application.Features.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Authentication controller for login, register, and profile management.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Login with username and password. Email is no longer a valid login
    /// identifier — users must sign in with their account username.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var command = new LoginCommand
            {
                Username = request.Username,
                Password = request.Password
            };
            var result = await _mediator.Send(command);
            return Ok(new { success = true, message = "Login successful", data = result });
        }
        catch (LoginFailedDueToUnverifiedEmailException)
        {
            // SECURITY FIX #1: Return specific error code for frontend to show
            // "Resend verification email" button
            return Unauthorized(new
            {
                success = false,
                message = "Please verify your email before logging in.",
                code = "EMAIL_NOT_VERIFIED",
                data = (object?)null
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            // "Invalid username or password" and "Username is required" bubble here
            // (controller-level try/catch prevents them from reaching the global middleware).
            return Unauthorized(new
            {
                success = false,
                message = ex.Message,
                code = "INVALID_CREDENTIALS",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var command = new RegisterCommand
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName,
            Phone = request.Phone,
            Address = request.Address,
            Profession = request.Profession,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender
        };
        var result = await _mediator.Send(command);
        return Ok(new { success = true, message = "Registration successful", data = result });
    }

    /// <summary>
    /// Get current authenticated user's profile (MIN-001).
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
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
    /// Logout current user.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(new { success = true, message = "Logout successful", data = (object?)null });
    }

    /// <summary>
    /// Request a password reset email.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var result = await _mediator.Send(new ForgotPasswordCommand { Email = request.Email });
        
        // SECURITY FIX #4: Return 429 with Retry-After header if throttled
        if (result.IsThrottled)
        {
            Response.Headers.Append("Retry-After", result.RetryAfterSeconds.ToString());
            return StatusCode(429, new 
            { 
                success = false, 
                message = result.Message ?? "Too many requests. Please try again later.",
                retryAfterSeconds = result.RetryAfterSeconds
            });
        }
        
        // Always return success to prevent email enumeration
        return Ok(new { success = true, message = "If an account with that email exists, a password reset email has been sent.", data = (object?)null });
    }

    /// <summary>
    /// Reset password using a token.
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        await _mediator.Send(new ResetPasswordCommand
        {
            Token = request.Token,
            NewPassword = request.NewPassword
        });
        return Ok(new { success = true, message = "Password has been reset successfully", data = (object?)null });
    }
}

/// <summary>
/// Login request payload. Users sign in with their <see cref="Username"/> only
/// (email is no longer accepted as a login identifier).
/// </summary>
public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Profession { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
}

public class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
