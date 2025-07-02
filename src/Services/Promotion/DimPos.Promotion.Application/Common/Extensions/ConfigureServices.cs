using DimPos.Basket.Application.Common.Protos;
using DimPos.Promotion.Application.Common.Behaviours;
using DimPos.Promotion.Application.Common.Utils;
using DimPos.Promotion.Application.Features.Campaign.Command.CreateCampaign;
using DimPos.Promotion.Application.Features.CampaignStore.Command;
using DimPos.Promotion.Application.Features.PromotionRule.Command.CreatePromotionRule;
using DimPos.Promotion.Application.Services.Implement;
using DimPos.Promotion.Application.Services.Interface;
using DimPos.Promotion.Domain.Models.Settings;
using DimPos.Store.Application.Common.Protos;
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
        services.AddScoped<IValidator<CreateCampaignCommand>, CreateCampaignCommandValidator>();
        services.AddScoped<IValidator<CreateCampaignStoreCommand>, CreateCampaignStoreCommandValidator>();
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });
        // services.AddHostedService<CheckCampaignExpiredService>();
        // services.Configure<HostOptions>(hostOptions =>
        // {
        //     hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        // });
        services.AddHttpContextAccessor();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddGrpc();
        services.AddGrpcServices(configuration);
        services.AddHealthChecks();
        return services;
    }
    public static IServiceCollection AddGrpcServices(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("GrpcSettings")
            .Get<GrpcSettings>();
        if (settings == null || string.IsNullOrEmpty(settings.BasketUrl))
            throw new ArgumentNullException("Grpc is not configured.");

        services.AddGrpcClient<BasketGrpcService.BasketGrpcServiceClient>(x =>
            {
                x.Address = new Uri(settings.BasketUrl);
            }
        );
        services.AddGrpcClient<StoreGrpcService.StoreGrpcServiceClient>(x =>
        {
            x.Address = new Uri(settings.StoreUrl);
        });
        return services;
    }
}