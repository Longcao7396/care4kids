using GiveAID.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Notifications.Commands.MarkAsRead;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public MarkNotificationAsReadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == request.NotificationId && n.UserId == request.UserId, cancellationToken);

        if (notification == null)
        {
            throw new KeyNotFoundException($"Notification {request.NotificationId} not found for user {request.UserId}");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
