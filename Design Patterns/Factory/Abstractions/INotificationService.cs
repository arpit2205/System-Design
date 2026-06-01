using Models;

namespace Abstractions;

public interface INotificationService
{
    Task SendNotificationAsync(NotificationRequest request, CancellationToken token);
}