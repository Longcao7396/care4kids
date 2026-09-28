using MediatR;

namespace GiveAID.Application.Features.Contacts.Commands.Delete;

/// <summary>
/// Handler for DeleteContactCommand.
/// Performs a soft delete (sets IsDeleted/DeletedAt) so the record is
/// excluded from normal queries but retained in the database.
/// </summary>
public class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteContactCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _context.ContactMessages.FindAsync(
            new object[] { request.ContactId }, cancellationToken);

        if (contact == null)
        {
            return false;
        }

        contact.IsDeleted = true;
        contact.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
