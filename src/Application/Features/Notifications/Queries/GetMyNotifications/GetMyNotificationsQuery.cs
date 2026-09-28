using MediatR;

namespace GiveAID.Application.Features.Notifications.Queries.GetMyNotifications;

public class GetMyNotificationsQuery : IRequest<GetMyNotificationsResult>
{
    public int UserId { get; set; }
    public bool? UnreadOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetMyNotificationsResult
{
    public IEnumerable<NotificationDto> Items { get; set; } = Array.Empty<NotificationDto>();
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
