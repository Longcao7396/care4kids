using GiveAID.Application.Features.Contacts.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.Contacts.Commands.Submit;

/// <summary>
/// Handler for SubmitContactCommand.
/// </summary>
public class SubmitContactCommandHandler : IRequestHandler<SubmitContactCommand, ContactDto>
{
    private readonly IApplicationDbContext _context;

    public SubmitContactCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ContactDto> Handle(SubmitContactCommand request, CancellationToken cancellationToken)
    {
        var contact = new ContactMessage
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Subject = request.Subject,
            Message = request.Message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.ContactMessages.Add(contact);
        await _context.SaveChangesAsync(cancellationToken);

        return new ContactDto
        {
            ContactId = contact.ContactId,
            Name = contact.Name,
            Email = contact.Email,
            Phone = contact.Phone,
            Subject = contact.Subject,
            Message = contact.Message,
            IsRead = contact.IsRead,
            CreatedAt = contact.CreatedAt
        };
    }
}
