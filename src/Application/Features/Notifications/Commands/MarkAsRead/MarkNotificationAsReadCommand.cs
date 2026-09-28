using MediatR;

namespace GiveAID.Application.Features.Notifications.Commands.MarkAsRead;

public class MarkNotificationAsReadCommand : IRequest<Unit>
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
}
