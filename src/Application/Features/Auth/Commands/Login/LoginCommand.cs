using GiveAID.Application.Features.Auth.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user by username + password.
/// </summary>
public class LoginCommand : IRequest<LoginResponseDto>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
