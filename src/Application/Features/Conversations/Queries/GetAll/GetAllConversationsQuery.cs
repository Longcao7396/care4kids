using GiveAID.Application.Features.Conversations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Conversations.Queries.GetAll;

/// <summary>
/// Query to get all conversations (admin).
/// </summary>
public class GetAllConversationsQuery : IRequest<IEnumerable<ConversationDto>>
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
