using MediatR;

namespace GiveAID.Application.Features.Conversations.Commands.Close;

/// <summary>
/// Handler for CloseConversationCommand.
/// </summary>
public class CloseConversationCommandHandler : IRequestHandler<CloseConversationCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CloseConversationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CloseConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _context.Conversations.FindAsync(new object[] { request.ConversationId }, cancellationToken);

        if (conversation == null)
        {
            throw new InvalidOperationException("Conversation not found.");
        }

        conversation.Status = "Closed";
        conversation.ClosedAt = DateTime.UtcNow;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
