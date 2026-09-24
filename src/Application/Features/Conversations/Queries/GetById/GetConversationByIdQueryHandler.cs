using GiveAID.Application.Features.Conversations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Conversations.Queries.GetById;

/// <summary>
/// Handler for GetConversationByIdQuery.
/// </summary>
public class GetConversationByIdQueryHandler : IRequestHandler<GetConversationByIdQuery, ConversationDto>
{
    private readonly IApplicationDbContext _context;

    public GetConversationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConversationDto> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _context.Conversations
            .Include(c => c.User)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.ConversationId == request.ConversationId, cancellationToken);

        if (conversation == null)
        {
            throw new InvalidOperationException("Conversation not found.");
        }

        return new ConversationDto
        {
            ConversationId = conversation.ConversationId,
            UserId = conversation.UserId,
            UserName = conversation.User?.FullName,
            Subject = conversation.Subject,
            ConversationType = conversation.ConversationType,
            Status = conversation.Status,
            Priority = conversation.Priority,
            AssignedTo = conversation.AssignedTo,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt ?? conversation.CreatedAt,
            ClosedAt = conversation.ClosedAt,
            Messages = conversation.Messages?.Select(m => new MessageDto
            {
                MessageId = m.MessageId,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                MessageText = m.MessageText,
                IsInternalNote = m.IsInternalNote,
                CreatedAt = m.CreatedAt
            }).ToList()
        };
    }
}
