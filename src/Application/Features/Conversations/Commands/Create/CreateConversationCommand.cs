using GiveAID.Application.Features.Conversations.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.Conversations.Commands.Create;

/// <summary>
/// Command to create a conversation.
/// </summary>
public class CreateConversationCommand : IRequest<ConversationDto>
{
    public int? UserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? ConversationType { get; set; }
    public string? Priority { get; set; }
    public string? InitialMessage { get; set; }
}
