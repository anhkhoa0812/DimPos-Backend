using DimPos.Catalog.Application.Common.Protos;
using DimPos.MenuCombo.Application.Common.Behaviours;
using DimPos.MenuCombo.Application.Common.Utils;
using DimPos.MenuCombo.Application.Features.BrandMenu.Command.CreateBrandMenu;
using DimPos.MenuCombo.Application.Features.BrandMenu.Command.UpdateBrandMenu;
using DimPos.MenuCombo.Application.Services.Implement;
using DimPos.MenuCombo.Application.Services.Interface;
using DimPos.MenuCombo.Domain.Models.Settings;
using DimPos.Store.Application.Common.Protos;
using FluentValidation;
using Mediator;

namespace DimPos.MenuCombo.Application.Common.Extensions;

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
        services.AddScoped<IValidator<CreateBrandMenuCommand>, CreateBrandMenuCommandValidator>();
        services.AddScoped<IValidator<UpdateBrandMenuCommand>, UpdateBrandMenuCommandValidator>();
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });
        services.AddCustomKafka(configuration);
        services.AddHttpContextAccessor();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddGrpcServices(configuration);
        
        services.AddHealthChecks();
        return services;
    }

    public static IServiceCollection AddGrpcServices(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("GrpcSettings")
            .Get<GrpcSettings>();
        if (settings == null || string.IsNullOrEmpty(settings.CatalogUrl))
            throw new ArgumentNullException("Grpc is not configured.");

        services.AddGrpcClient<CatalogGrpcService.CatalogGrpcServiceClient>(x =>
            {
                x.Address = new Uri(settings.CatalogUrl);
            }
        );
        services.AddGrpcClient<StoreGrpcService.StoreGrpcServiceClient>(x =>
        {
            x.Address = new Uri(settings.StoreUrl);
        });

        
        return services;
    }
}