using MediatR;

namespace GiveAID.Application.Features.Users.Commands.DeactivateUser;

/// <summary>
/// Command to deactivate a user.
/// </summary>
public class DeactivateUserCommand : IRequest<bool>
{
    public int UserId { get; set; }
}
