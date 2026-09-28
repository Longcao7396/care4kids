using GiveAID.Application.Features.Auth.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new user.
/// </summary>
public class RegisterCommand : IRequest<UserDto>
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
