using GiveAID.Application.Common.Interfaces;
using GiveAID.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Notifications.Queries.GetMyNotifications;

public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, GetMyNotificationsResult>
{
    private readonly IApplicationDbContext _context;

    public GetMyNotificationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetMyNotificationsResult> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == request.UserId);

        if (request.UnreadOnly == true)
        {
            query = query.Where(n => !n.IsRead);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Get unread count
        var unreadCount = await _context.Notifications
            .Where(n => n.UserId == request.UserId && !n.IsRead)
            .CountAsync(cancellationToken);

        // Apply pagination
        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => new NotificationDto
            {
                Id = n.NotificationId,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                RelatedEntityType = n.RelatedEntityType,
                RelatedEntityId = n.RelatedEntityId,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new GetMyNotificationsResult
        {
            Items = notifications,
            TotalCount = totalCount,
            UnreadCount = unreadCount
        };
    }
}
