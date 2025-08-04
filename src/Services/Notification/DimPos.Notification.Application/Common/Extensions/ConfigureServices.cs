using DimPos.Notification.Application.Common.Behaviours;
using DimPos.Notification.Application.Common.Utils;
using DimPos.Notification.Application.Services.Implement;
using DimPos.Notification.Application.Services.Interface;
using Mediator;

namespace DimPos.Notification.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediator(options =>
            {
                options.Namespace = "DimPos.Notification.Application.Endpoints";
                options.ServiceLifetime = ServiceLifetime.Scoped;
            })
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddScoped(typeof(ValidationUtil<>));
        services.AddSignalR(options => { options.EnableDetailedErrors = true; });

        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });
        services.AddCustomKafka(configuration);
        services.AddHttpContextAccessor();

        return services;
    }

}