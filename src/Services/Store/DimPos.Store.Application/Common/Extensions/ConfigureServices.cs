using DimPos.Identity.Application.Common.Protos;
using DimPos.Payment.Application.Common.Protos;
using DimPos.Store.Application.Common.Behaviours;
using DimPos.Store.Application.Common.Utils;
using DimPos.Store.Application.Features.FinancialShift.Command.OpenFinancialShift;
using DimPos.Store.Application.Features.FinancialShiftConfig.Command.CreateFinancialShiftConfig;
using DimPos.Store.Application.Features.FinancialShiftConfig.Command.UpdateFinancialShiftConfig;
using DimPos.Store.Application.Features.StorePaymentMethodConfig.Command.CreateStorePaymentMethodConfig;
using DimPos.Store.Application.Features.StorePaymentMethodConfig.Command.UpdateStorePaymentConfig;
using DimPos.Store.Application.Features.Stores.Command.CreateStaff;
using DimPos.Store.Application.Features.Stores.Command.CreateStore;
using DimPos.Store.Application.Features.Stores.Command.UpdateStaff;
using DimPos.Store.Application.Features.Stores.Command.UpdateStore;
using DimPos.Store.Application.Features.TaxRate.Command.CreateTaxRate;
using DimPos.Store.Application.Features.TaxRate.Command.UpdateTaxRate;
using DimPos.Store.Application.Services.Implement;
using DimPos.Store.Application.Services.Interface;
using DimPos.Store.Domain.Models.Settings;
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
        services.AddScoped<IValidator<CreateStaffCommand>, CreateStaffCommandValidator>();
        services.AddScoped<IValidator<OpenFinancialShiftCommand>, OpenFinancialShiftCommandValidator>();
        services.AddScoped<IValidator<CreateTaxRateCommand>, CreateTaxRateCommandValidator>();
        services.AddScoped<IValidator<UpdateTaxRateCommand>, UpdateTaxRateCommandValidator>();
        services.AddScoped<IValidator<CreateFinancialShiftConfigCommand>, CreateFinancialShiftConfigCommandValidator>();
        services.AddScoped<IValidator<UpdateFinancialShiftConfigCommand>, UpdateFinancialShiftConfigCommandValidator>();
        services.AddScoped<IValidator<OpenFinancialShiftCommand>, OpenFinancialShiftCommandValidator>();
        services.AddScoped<IValidator<UpdateStorePaymentConfigCommand>, UpdateStorePaymentConfigCommandValidator>();
        services.AddScoped<IValidator<UpdateStaffCommand>, UpdateStaffCommandValidator>();
        services.AddScoped<IValidator<UpdateStoreCommand>, UpdateStoreCommandValidator>();
        services
            .AddScoped<IValidator<CreateStorePaymentMethodConfigCommand>,
                CreateStorePaymentMethodConfigCommandValidator>();
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });
        services.AddCustomKafka(configuration);
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
        if (settings == null || string.IsNullOrEmpty(settings.PaymentUrl))
            throw new ArgumentNullException("Grpc is not configured.");

        services.AddGrpcClient<PaymentGrpcService.PaymentGrpcServiceClient>(x =>
            {
                x.Address = new Uri(settings.PaymentUrl);
            }
        );
        services.AddGrpcClient<IdentityGrpcService.IdentityGrpcServiceClient>(x =>
        {
            x.Address = new Uri(settings.IdentityUrl);
        });
        return services;
    }
}