using GiveAID.Application.Features.Contacts.DTOs;

namespace GiveAID.Application.Features.Contacts.Queries.GetById;

/// <summary>
/// Handler for GetContactByIdQuery.
/// </summary>
public class GetContactByIdQueryHandler : IRequestHandler<GetContactByIdQuery, ContactDto>
{
    private readonly IApplicationDbContext _context;

    public GetContactByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ContactDto> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
    {
        var contact = await _context.ContactMessages.FindAsync(new object[] { request.ContactId }, cancellationToken);

        if (contact == null)
        {
            throw new InvalidOperationException($"Contact message with ID {request.ContactId} not found.");
        }

        // Mark as read
        if (!contact.IsRead)
        {
            contact.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new ContactDto
        {
            ContactId = contact.ContactId,
            Name = contact.Name,
            Email = contact.Email,
            Phone = contact.Phone,
            Subject = contact.Subject,
            Message = contact.Message,
            IsRead = contact.IsRead,
            RepliedBy = contact.RepliedBy,
            ReplyMessage = contact.ReplyMessage,
            RepliedAt = contact.RepliedAt,
            CreatedAt = contact.CreatedAt
        };
    }
}
