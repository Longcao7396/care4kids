using GiveAID.Application.Features.Contacts.DTOs;
using GiveAID.Application.Common.Interfaces;
using MediatR;

namespace GiveAID.Application.Features.Contacts.Commands.Reply;

/// <summary>
/// Handler for ReplyContactCommand.
/// Sends the reply by email to the original sender (IEmailSender also logs
/// the send via IEmailLogService internally) and records the reply on the
/// ContactMessage entity.
/// </summary>
public class ReplyContactCommandHandler : IRequestHandler<ReplyContactCommand, ContactDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _emailSender;

    public ReplyContactCommandHandler(
        IApplicationDbContext context,
        IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    public async Task<ContactDto> Handle(ReplyContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _context.ContactMessages.FindAsync(
            new object[] { request.ContactId }, cancellationToken);

        if (contact == null)
        {
            throw new KeyNotFoundException($"Contact message with ID {request.ContactId} not found");
        }

        var subject = $"Re: {contact.Subject ?? "Your message to GiveAID"}";
        var body = $"<p>Hi {contact.Name},</p><p>{request.ReplyMessage}</p><p>— GiveAID Support</p>";

        await _emailSender.SendEmailAsync(contact.Email, subject, body, isHtml: true);

        contact.ReplyMessage = request.ReplyMessage;
        contact.RepliedBy = request.RepliedBy;
        contact.RepliedAt = DateTime.UtcNow;
        contact.IsRead = true;

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
            RepliedBy = contact.RepliedBy,
            ReplyMessage = contact.ReplyMessage,
            RepliedAt = contact.RepliedAt,
            CreatedAt = contact.CreatedAt
        };
    }
}
