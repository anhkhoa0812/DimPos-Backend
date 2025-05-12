using DimPos.MenuCombo.Application.Common.Behaviours;
using DimPos.MenuCombo.Application.Common.Utils;
using Mediator;

namespace DimPos.MenuCombo.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediator(options =>
            {
                options.Namespace = "DimPos.Store.Application.Endpoints";
                options.ServiceLifetime = ServiceLifetime.Scoped;
            })
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddScoped(typeof(ValidationUtil<>));
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });

        services.AddHttpContextAccessor();
        
        services.AddHealthChecks();
        return services;
    }
}