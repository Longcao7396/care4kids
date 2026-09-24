using MediatR;

namespace GiveAID.Application.Features.Conversations.Commands.Close;

/// <summary>
/// Command to close a conversation.
/// </summary>
public class CloseConversationCommand : IRequest<bool>
{
    public int ConversationId { get; set; }
}
