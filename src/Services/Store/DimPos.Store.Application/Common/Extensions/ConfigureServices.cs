using DimPos.Store.Application.Common.Behaviours;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Features.Stores.Command.CreateStore;
using DimPos.Store.Application.Services.Implement;
using DimPos.Store.Application.Services.Interface;
using FluentValidation;
using Mediator;

namespace DimPos.Store.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediator(options =>
            {
                options.Namespace = "DimPos.Store.Application.Endpoints";
                options.ServiceLifetime = ServiceLifetime.Scoped;
            })
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddScoped(typeof(ValidationUtil<>));
        services.AddScoped<IValidator<CreateStoreCommand>, CreateStoreCommandValidator>();
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });
        services.AddCustomKafka(configuration);
        services.AddGrpc();
        services.AddHttpContextAccessor();
        services.AddScoped<IClaimService, ClaimService>();
        
        services.AddHealthChecks();
        return services;
    }
}