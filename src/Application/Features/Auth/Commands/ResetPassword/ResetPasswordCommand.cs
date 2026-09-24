using MediatR;

namespace GiveAID.Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Command to reset a user's password.
/// </summary>
public class ResetPasswordCommand : IRequest<bool>
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
