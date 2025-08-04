using DimPos.Notification.Infrastructure.Configurations;
using DimPos.Notification.Infrastructure.Persistence;
using DimPos.Notification.Infrastructure.Repositories;
using DimPos.Notification.Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DimPos.Notification.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NotificationContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString"),
                builder => builder.MigrationsAssembly(typeof(NotificationContext).Assembly.FullName));
        });
        services.AddScoped<IUnitOfWork<NotificationContext>, UnitOfWork<NotificationContext>>();
        services.AddScoped<NotificationContextSeed>();
        services.AddJWT(configuration);
        services.AddOpenApiConfig();
        services.AddAuthorization();
        services.AddEndpointsApiExplorer();
        // services.AddCors();
        return services;
    }
}