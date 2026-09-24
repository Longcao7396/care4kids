using MediatR;

namespace GiveAID.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Command to request a password reset.
/// </summary>
public class ForgotPasswordCommand : IRequest<ForgotPasswordResult>
{
    public string Email { get; set; } = string.Empty;
}
