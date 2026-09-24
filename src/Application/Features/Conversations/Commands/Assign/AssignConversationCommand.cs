using MediatR;

namespace GiveAID.Application.Features.Conversations.Commands.Assign;

/// <summary>
/// Command to assign a conversation to an admin.
/// </summary>
public class AssignConversationCommand : IRequest<bool>
{
    public int ConversationId { get; set; }
    public int AssignedTo { get; set; }
}
