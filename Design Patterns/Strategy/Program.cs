using Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;
using Strategies;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddOptions();

// Register services to DI
builder.Services.AddTransient<Application>();
builder.Services.AddSingleton<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentStrategy, CreditCardPaymentStrategy>();
builder.Services.AddScoped<IPaymentStrategy, UpiPaymentStrategy>();
builder.Services.AddScoped<IPaymentStrategyResolver, PaymentStrategyResolver>();

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