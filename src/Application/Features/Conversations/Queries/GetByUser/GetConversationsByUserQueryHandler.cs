using GiveAID.Application.Features.Conversations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Conversations.Queries.GetByUser;

/// <summary>
/// Handler for GetConversationsByUserQuery.
/// </summary>
public class GetConversationsByUserQueryHandler : IRequestHandler<GetConversationsByUserQuery, IEnumerable<ConversationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetConversationsByUserQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ConversationDto>> Handle(GetConversationsByUserQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _context.Conversations
            .Include(c => c.User)
            .Where(c => c.UserId == request.UserId)
            .OrderByDescending(c => c.UpdatedAt)
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
