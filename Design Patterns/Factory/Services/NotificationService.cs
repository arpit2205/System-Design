using Abstractions;
using Models;

namespace Services;

public class NotificationService : INotificationService
{
    private readonly INotificationFactory _notificationFactory;

    public NotificationService(INotificationFactory notificationFactory)
    {
        _notificationFactory = notificationFactory;
    }
    public async Task SendNotificationAsync(NotificationRequest request, CancellationToken token)
    {
        INotificationSender sender = _notificationFactory.Create(request.NotificationType);
        await sender.Notify(request);
    }
}