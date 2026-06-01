using Models;

namespace Abstractions;

public interface INotificationFactory
{
    INotificationSender Create(string notificationType);
}