using GiveAID.Application.Features.Contacts.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Contacts.Commands.Reply;

/// <summary>
/// Command to reply to a contact message. Sends the reply by email to the
/// original sender and records the reply on the ContactMessage.
/// </summary>
public class ReplyContactCommand : IRequest<ContactDto>
{
    public int ContactId { get; set; }
    public string ReplyMessage { get; set; } = string.Empty;
    public int? RepliedBy { get; set; }
}
