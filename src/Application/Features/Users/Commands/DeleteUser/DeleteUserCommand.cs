using MediatR;

namespace GiveAID.Application.Features.Users.Commands.DeleteUser;

/// <summary>
/// Command to soft-delete a user.
/// </summary>
public class DeleteUserCommand : IRequest<bool>
{
    public int UserId { get; set; }
}
