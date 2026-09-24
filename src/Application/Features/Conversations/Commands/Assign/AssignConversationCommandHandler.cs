using MediatR;

namespace GiveAID.Application.Features.Conversations.Commands.Assign;

/// <summary>
/// Handler for AssignConversationCommand.
/// </summary>
public class AssignConversationCommandHandler : IRequestHandler<AssignConversationCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public AssignConversationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AssignConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _context.Conversations.FindAsync(new object[] { request.ConversationId }, cancellationToken);

        if (conversation == null)
        {
            throw new InvalidOperationException("Conversation not found.");
        }

        conversation.AssignedTo = request.AssignedTo;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
