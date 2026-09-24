using GiveAID.Application.Features.Contacts.DTOs;

namespace GiveAID.Application.Features.Contacts.Queries.GetAll;

/// <summary>
/// Handler for GetAllContactsQuery.
/// </summary>
public class GetAllContactsQueryHandler : IRequestHandler<GetAllContactsQuery, IEnumerable<ContactDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllContactsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ContactDto>> Handle(GetAllContactsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ContactMessages.AsQueryable();

        if (request.IsRead.HasValue)
        {
            query = query.Where(c => c.IsRead == request.IsRead.Value);
        }

        var contacts = await _context.ContactMessages
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return contacts.Select(c => new ContactDto
        {
            ContactId = c.ContactId,
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone,
            Subject = c.Subject,
            Message = c.Message,
            IsRead = c.IsRead,
            RepliedBy = c.RepliedBy,
            ReplyMessage = c.ReplyMessage,
            RepliedAt = c.RepliedAt,
            CreatedAt = c.CreatedAt
        });
    }
}
