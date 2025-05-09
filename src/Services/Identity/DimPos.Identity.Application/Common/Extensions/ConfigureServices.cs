using DimPos.Identity.Application.Common.Behaviours;
using DimPos.Identity.Application.Common.Config;
using DimPos.Identity.Application.Common.Utils;
using DimPos.Identity.Application.Features.Authentication.Command.Login;
using DimPos.Identity.Application.Services.Implement;
using DimPos.Identity.Application.Services.Interface;
using FluentValidation;
using Mediator;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace DimPos.Identity.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddMediator(options =>
            {
                options.Namespace = "DimPos.Identity.Application.Endpoints";
                options.ServiceLifetime = ServiceLifetime.Scoped;
            })
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddScoped(typeof(ValidationUtil<>));
        services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
        
        services.Configure<RouteHandlerOptions>(options => { options.ThrowOnBadRequest = true; });
        services.AddCustomKafka(configuration);

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        
        services.AddHealthChecks();
        
        return services;
    }
}