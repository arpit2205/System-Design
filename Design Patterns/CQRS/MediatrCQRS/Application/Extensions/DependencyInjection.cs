using System.Reflection;
using Application.Common.Interfaces;
using Infrastructure.Repositories;

namespace Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IProductRepository, ProductRepository>();
        services.AddMediatR(cfg =>
        {   
            cfg.RegisterServicesFromAssembly(
            Assembly.GetExecutingAssembly());
        });

        return services;
    }
}