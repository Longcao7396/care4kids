using GiveAID.Application.Common.Interfaces;
using GiveAID.Application.Features.Users.Commands.CreateAdmin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for bootstrapping the application with initial admin user.
/// </summary>
[ApiController]
[Route("api/v1/auth/bootstrap")]
public class AuthBootstrapController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IApplicationDbContext _context;

    public AuthBootstrapController(ISender mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    /// <summary>
    /// Bootstrap the application with initial admin user (Admin only).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Bootstrap([FromBody] BootstrapRequest request)
    {
        var command = new CreateAdminCommand
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName
        };

        var result = await _mediator.Send(command);
        return Ok(new { success = true, message = "Admin user created", data = result });
    }

    /// <summary>
    /// Check if bootstrap is needed (public).
    /// </summary>
    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatus()
    {
        var hasUsers = _context.Users.Any();
        return Ok(new { success = true, message = "OK", data = new { needsBootstrap = !hasUsers } });
    }
}

public class BootstrapRequest
{
    public string Username { get; set; } = "admin";
    public string Email { get; set; } = "admin@give-aid.org";
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = "System Administrator";
}
