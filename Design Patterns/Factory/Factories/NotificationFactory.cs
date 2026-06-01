using Abstractions;
using Concrete;
using Microsoft.Extensions.DependencyInjection;

namespace Factories;

public class NotificationFactory : INotificationFactory
{
    // Either we can have IServiceProvider and get service type directly
    // Or we can inject all implementations of INotificationSender, create a dictionary and filter, just like strategy pattern
    private readonly IServiceProvider serviceProvider;

    public NotificationFactory(IServiceProvider provider)
    {
        serviceProvider = provider;
    }
    public INotificationSender Create(string notificationType)
    {
        // Since it is Factory pattern - we care about creation of service, not about its abstraction (interface)
        return notificationType switch
        {
            "Email" => serviceProvider.GetRequiredService<EmailNotificationSender>(),
            "Sms" => serviceProvider.GetRequiredService<SmsNotificationSender>(),
            _ => throw new NotSupportedException()
        };
    }
}