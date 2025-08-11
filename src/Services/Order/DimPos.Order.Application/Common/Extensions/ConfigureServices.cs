using DimPos.Catalog.Application.Common.Protos;
using DimPos.Identity.Application.Common.Protos;
using DimPos.Inventory.Application.Common.Protos;
using DimPos.Order.Application.Common.Behaviours;
using DimPos.Order.Application.Common.Utils;
using DimPos.Order.Application.Features.Order.Command.CancelOrder;
using DimPos.Order.Application.Features.Order.Command.ConfirmCashOrder;
using DimPos.Order.Application.Features.Order.Command.CreateOrder;
using DimPos.Order.Application.Features.Order.Command.UpdatePaymentMethod;
using DimPos.Order.Application.Features.StorePurchaseOrder.Command.CreateStorePurchaseOrder;
using DimPos.Order.Application.Features.StorePurchaseOrder.Command.UpdateStorePurchaseOrder;
using DimPos.Order.Application.Services.Implement;
using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Models.Settings;
using DimPos.Payment.Application.Common.Protos;
using DimPos.Promotion.Application.Common.Protos;
using DimPos.Store.Application.Common.Protos;
using FluentValidation;
using Mediator;

namespace DimPos.Order.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediator(options =>
            {
                options.Namespace = "DimPos.Order.Application.Endpoints";
                options.ServiceLifetime = ServiceLifetime.Scoped;
            })
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddScoped(typeof(ValidationUtil<>));
        services.AddScoped<IValidator<CreateOrderCommand>, CreateOrderCommandValidator>();
        services.AddScoped<IValidator<UpdatePaymentMethodCommand>, UpdatePaymentMethodCommandValidator>();
        services.AddScoped<IValidator<CreateStorePurchaseOrderCommand>, CreateStorePurchaseOrderCommandValidator>();
        services.AddScoped<IValidator<UpdateStorePurchaseOrderCommand>, UpdateStorePurchaseOrderCommandValidator>();
        services.AddScoped<IValidator<ConfirmCashOrderCommand>, ConfirmCashOrderCommandValidator>();
        services.AddScoped<IValidator<CancelOrderCommand>, CancelOrderCommandValidator>();
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });
        services.AddGrpc();
        services.AddHttpContextAccessor();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddCustomKafka(configuration);
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
        services.AddGrpcClient<PromotionGrpcService.PromotionGrpcServiceClient>(x =>
        {
            x.Address = new Uri(settings.PromotionUrl);
        });
        services.AddGrpcClient<PaymentGrpcService.PaymentGrpcServiceClient>(x =>
        {
            x.Address = new Uri(settings.PaymentUrl);
        });
        services.AddGrpcClient<InventoryGrpcService.InventoryGrpcServiceClient>(x =>
        {
            x.Address = new Uri(settings.InventoryUrl);
        });
        services.AddGrpcClient<IdentityGrpcService.IdentityGrpcServiceClient>(x =>
        {
            x.Address = new Uri(settings.IdentityUrl);
        });
            
        return services;
    }
}