using GiveAID.Domain.Entities;

namespace GiveAID.Application.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(
        int userId,
        string type,
        string title,
        string message,
        string? relatedEntityType = null,
        int? relatedEntityId = null,
        CancellationToken cancellationToken = default);
}
