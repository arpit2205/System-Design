using Abstractions;
using Models;

namespace Concrete;

public class EmailNotificationSender : INotificationSender
{
    public Task Notify(NotificationRequest request)
    {
        System.Console.WriteLine($"[NOTIFICATION] [{System.DateTime.UtcNow:O}] Type={request.NotificationType ?? "N/A"} | From={request.Sender ?? "unknown"} -> To={request.Receiver ?? "unknown"} | Message=\"{request.Message ?? ""}\"");

        return Task.CompletedTask;
    }
}