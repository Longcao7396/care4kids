using GiveAID.Application.Features.Conversations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Conversations.Queries.GetByUser;

/// <summary>
/// Query to get conversations by user ID.
/// </summary>
public class GetConversationsByUserQuery : IRequest<IEnumerable<ConversationDto>>
{
    public int UserId { get; set; }
}
