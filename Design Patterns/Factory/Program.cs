using Abstractions;
using Concrete;
using Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddOptions();

// Register services to DI
builder.Services.AddTransient<Application>();
builder.Services.AddSingleton<INotificationFactory, NotificationFactory>();
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddScoped<EmailNotificationSender>();
builder.Services.AddScoped<SmsNotificationSender>();

var host = builder.Build();

try
{
    var app = host.Services.GetRequiredService<Application>();
    await app.RunAsync();
}
catch (Exception ex)
{
    System.Console.WriteLine(ex);
}
finally
{
    await host.StopAsync();
}