using GiveAID.Application.Features.Contacts.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Contacts.Queries.GetAll;

/// <summary>
/// Query to get all contact messages (admin).
/// </summary>
public class GetAllContactsQuery : IRequest<IEnumerable<ContactDto>>
{
    public bool? IsRead { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
