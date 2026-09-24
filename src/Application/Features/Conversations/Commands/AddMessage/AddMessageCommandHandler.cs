using GiveAID.Application.Features.Conversations.DTOs;
using GiveAID.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Conversations.Commands.AddMessage;

/// <summary>
/// Handler for AddMessageCommand.
/// </summary>
public class AddMessageCommandHandler : IRequestHandler<AddMessageCommand, MessageDto>
{
    private readonly IApplicationDbContext _context;

    public AddMessageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MessageDto> Handle(AddMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _context.Conversations.FindAsync(new object[] { request.ConversationId }, cancellationToken);
        if (conversation == null)
        {
            throw new InvalidOperationException("Conversation not found.");
        }

        var message = new ConversationMessage
        {
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            MessageText = request.MessageText,
            IsInternalNote = request.IsInternalNote,
            CreatedAt = DateTime.UtcNow
        };

        _context.ConversationMessages.Add(message);

        conversation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var sender = await _context.Users.FindAsync(new object[] { request.SenderId }, cancellationToken);

        return new MessageDto
        {
            MessageId = message.MessageId,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            SenderName = sender?.FullName,
            MessageText = message.MessageText,
            IsInternalNote = message.IsInternalNote,
            CreatedAt = message.CreatedAt
        };
    }
}
