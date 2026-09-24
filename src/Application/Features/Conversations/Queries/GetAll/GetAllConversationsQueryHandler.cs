using GiveAID.Application.Features.Conversations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Conversations.Queries.GetAll;

/// <summary>
/// Handler for GetAllConversationsQuery.
/// </summary>
public class GetAllConversationsQueryHandler : IRequestHandler<GetAllConversationsQuery, IEnumerable<ConversationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllConversationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ConversationDto>> Handle(GetAllConversationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Conversations
            .Include(c => c.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(c => c.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.Priority))
        {
            query = query.Where(c => c.Priority == request.Priority);
        }

        var conversations = await query
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return conversations.Select(c => new ConversationDto
        {
            ConversationId = c.ConversationId,
            UserId = c.UserId,
            UserName = c.User?.FullName,
            Subject = c.Subject,
            ConversationType = c.ConversationType,
            Status = c.Status,
            Priority = c.Priority,
            AssignedTo = c.AssignedTo,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt ?? c.CreatedAt,
            ClosedAt = c.ClosedAt
        });
    }
}
