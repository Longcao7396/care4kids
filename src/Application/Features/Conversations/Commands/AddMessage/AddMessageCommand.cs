using GiveAID.Application.Features.Conversations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Conversations.Commands.AddMessage;

/// <summary>
/// Command to add a message to a conversation.
/// </summary>
public class AddMessageCommand : IRequest<MessageDto>
{
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public bool IsInternalNote { get; set; }
}
