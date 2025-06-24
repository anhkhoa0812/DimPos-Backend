using DimPos.Promotion.Application.Common.Behaviours;
using DimPos.Promotion.Application.Common.Utils;
using DimPos.Promotion.Application.Features.PromotionRule.Command.CreatePromotionRule;
using DimPos.Promotion.Application.Services.Implement;
using DimPos.Promotion.Application.Services.Interface;
using FluentValidation;
using Mediator;

namespace DimPos.Promotion.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediator(options =>
            {
                options.Namespace = "DimPos.Promotion.Application.Endpoints";
                options.ServiceLifetime = ServiceLifetime.Scoped;
            })
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddScoped(typeof(ValidationUtil<>));
        services.AddScoped<IValidator<CreatePromotionRuleCommand>, CreatePromotionRuleCommandValidator>();
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