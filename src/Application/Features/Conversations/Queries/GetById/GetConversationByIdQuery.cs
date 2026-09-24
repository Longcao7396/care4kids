using GiveAID.Application.Features.Conversations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Conversations.Queries.GetById;

/// <summary>
/// Query to get a conversation by ID with messages.
/// </summary>
public class GetConversationByIdQuery : IRequest<ConversationDto>
{
    public int ConversationId { get; set; }
}
