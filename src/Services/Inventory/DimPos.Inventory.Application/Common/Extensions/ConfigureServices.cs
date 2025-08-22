using DimPos.Catalog.Application.Common.Protos;
using DimPos.Inventory.Application.Common.Behaviours;
using DimPos.Inventory.Application.Common.Utils;
using DimPos.Inventory.Application.Features.InventoryStock.Command.UpdateQuantiyOfInventoryStock;
using DimPos.Inventory.Application.Services.Implement;
using DimPos.Inventory.Application.Services.Interface;
using DimPos.Inventory.Domain.Models.Settings;
using DimPos.Order.Application.Common.Protos;
using DimPos.Store.Application.Common.Protos;
using FluentValidation;
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
        services
            .AddScoped<IValidator<UpdateQuantityOfInventoryStockCommand>,
                UpdateQuantityOfInventoryStockCommandValidator>();
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });
        services.AddCustomKafka(configuration);
        services.AddGrpc();
        services.AddGrpcServices(configuration);
        services.AddHttpContextAccessor();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<IHangfireService, HangfireService>();
        services.AddHealthChecks();
        return services;
    }
    public static IServiceCollection AddGrpcServices(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("GrpcSettings")
            .Get<GrpcSettings>();
        if (settings == null || string.IsNullOrEmpty(settings.OrderUrl))
            throw new ArgumentNullException("Grpc is not configured.");

        services.AddGrpcClient<OrderGrpcService.OrderGrpcServiceClient>(x =>
            {
                x.Address = new Uri(settings.OrderUrl);
            }
        );
        services.AddGrpcClient<CatalogGrpcService.CatalogGrpcServiceClient>(x =>
        {
            x.Address = new Uri(settings.CatalogUrl);
        });
        services.AddGrpcClient<StoreGrpcService.StoreGrpcServiceClient>(x =>
        {
            x.Address = new Uri(settings.StoreUrl);
        });
        return services;
    }
}