using Abstractions;
using Models;

public class Application
{
    private readonly INotificationService _notificationService;
    public Application(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    public async Task RunAsync()
    {
        var emailNotificationRequest = new NotificationRequest("Email", "Arpit", "Bot", "Hello");
        var smsNotificationRequest = new NotificationRequest("Sms", "Bot", "Arpit", "11001");
        CancellationToken token = new CancellationToken();

        await _notificationService.SendNotificationAsync(emailNotificationRequest, token);
        await _notificationService.SendNotificationAsync(smsNotificationRequest, token);

        // return Task.CompletedTask;
    }
}