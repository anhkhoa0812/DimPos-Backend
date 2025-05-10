using DimPos.Brand.Application.Common.Behaviours;
using DimPos.Brand.Application.Common.Protos;
using DimPos.Brand.Application.Common.Utils;
using DimPos.Brand.Application.Features.Brands.Command;
using DimPos.Brand.Domain.Models.Settings;
using FluentValidation;
using Mediator;

namespace DimPos.Brand.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.Namespace = "DimPos.Brand.Application.Endpoints";
            options.ServiceLifetime = ServiceLifetime.Scoped;
        })
        .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>))
        .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddScoped(typeof(ValidationUtil<>));
        services.AddScoped<IValidator<CreateBrandCommand>, CreateBrandCommandValidator>();
        
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });
        services.AddGrpc();
        services.AddHealthChecks();
        return services;
    }

}