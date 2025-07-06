using DimPos.Inventory.Application.Common.Behaviours;
using DimPos.Inventory.Application.Common.Utils;
using DimPos.Inventory.Application.Services.Implement;
using DimPos.Inventory.Application.Services.Interface;
using Mediator;

namespace DimPos.Inventory.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediator(options =>
            {
                options.Namespace = "DimPos.Inventory.Application.Endpoints";
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
        services.AddScoped<IClaimService, ClaimService>();
        services.AddHealthChecks();
        return services;
    }
}