using GiveAID.Application.Features.Contacts.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Contacts.Queries.GetById;

/// <summary>
/// Query to get a contact message by ID.
/// </summary>
public class GetContactByIdQuery : IRequest<ContactDto>
{
    public int ContactId { get; set; }
}
