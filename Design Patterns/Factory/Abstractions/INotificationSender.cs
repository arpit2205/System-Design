using Models;

namespace Abstractions;

public interface INotificationSender
{
    Task Notify(NotificationRequest request);
}