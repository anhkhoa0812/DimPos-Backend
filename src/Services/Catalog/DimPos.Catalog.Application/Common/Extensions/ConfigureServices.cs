using DimPos.Catalog.Application.Common.Behaviours;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.Categories.Command.CreateCategories;
using DimPos.Catalog.Application.Features.ModifierGroups.Command.CreateModifierGroups;
using DimPos.Catalog.Application.Features.Products.Commands.CreateProducts;
using DimPos.Catalog.Application.Services.Implement;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Settings;
using DimPos.Media.Application.Common.Protos;
using FluentValidation;
using Mediator;

namespace DimPos.Catalog.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMediator( options =>
            {
                options.Namespace = "DimPos.Catalog.Application.Endpoints";
                options.ServiceLifetime = ServiceLifetime.Scoped;
            })
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddScoped(typeof(ValidationUtil<>));
        services.AddScoped<IValidator<CreateProductsCommand>, CreateProductsCommandValidator>();
        services.AddScoped<IValidator<CreateCategoriesCommand>, CreateCategoriesCommandValidator>();
        services.AddScoped<IValidator<CreateModifierGroupsCommand>, CreateModifierGroupsCommandValidator>();
        
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });
        services.AddGrpc();
        services.AddGrpcServices(configuration);
        services.AddHttpContextAccessor();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddHealthChecks();
        return services;
    }
    public static IServiceCollection AddGrpcServices(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("GrpcSettings")
            .Get<GrpcSettings>();
        if (settings == null || string.IsNullOrEmpty(settings.MediaUrl))
            throw new ArgumentNullException("Grpc is not configured.");

        services.AddGrpcClient<MediaGrpcService.MediaGrpcServiceClient>(x =>
            {
                x.Address = new Uri(settings.MediaUrl);
                x.ChannelOptionsActions.Add(channelOptions =>
                {
                    channelOptions.HttpVersion = System.Net.HttpVersion.Version20;
                });
            }
        );
        return services;
    }
}