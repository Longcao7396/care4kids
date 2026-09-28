using MediatR;

namespace GiveAID.Application.Features.Contacts.Commands.Delete;

/// <summary>
/// Command to soft-delete a contact message.
/// </summary>
public class DeleteContactCommand : IRequest<bool>
{
    public int ContactId { get; set; }
}
