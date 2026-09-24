using GiveAID.Application.Features.Conversations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Conversations.Commands.Create;

/// <summary>
/// Handler for CreateConversationCommand.
/// </summary>
public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    private readonly IApplicationDbContext _context;

    public CreateConversationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = new Domain.Entities.Conversation
        {
            UserId = request.UserId,
            Subject = request.Subject,
            ConversationType = request.ConversationType ?? "Support",
            Priority = request.Priority ?? "Normal",
            Status = "Open",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);

        // Add initial message if provided
        if (!string.IsNullOrEmpty(request.InitialMessage))
        {
            var message = new Domain.Entities.ConversationMessage
            {
                ConversationId = conversation.ConversationId,
                SenderId = request.UserId ?? 0,
                MessageText = request.InitialMessage,
                IsInternalNote = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.ConversationMessages.Add(message);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new ConversationDto
        {
            ConversationId = conversation.ConversationId,
            UserId = conversation.UserId,
            Subject = conversation.Subject,
            ConversationType = conversation.ConversationType,
            Status = conversation.Status,
            Priority = conversation.Priority,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt ?? conversation.CreatedAt
        };
    }
}
