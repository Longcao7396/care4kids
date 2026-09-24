using MediatR;

namespace GiveAID.Application.Features.Users.Commands.DeleteUser;

/// <summary>
/// Handler for DeleteUserCommand.
/// Performs a soft delete by setting IsActive = false and DeletedAt.
/// </summary>
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(
            new object[] { request.UserId }, cancellationToken);

        if (user == null)
        {
            return false;
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
