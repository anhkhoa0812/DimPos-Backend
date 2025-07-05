using DimPos.Payment.Application.Common.Behaviours;
using DimPos.Payment.Application.Common.Utils;
using DimPos.Payment.Application.Services.Implement;
using DimPos.Payment.Application.Services.Interface;
using DimPos.Payment.Domain.Settings;
using DimPos.Store.Application.Common.Protos;
using Mediator;

namespace DimPos.Payment.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediator(options =>
            {
                options.Namespace = "DimPos.Promotion.Application.Endpoints";
                options.ServiceLifetime = ServiceLifetime.Scoped;
            })
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddScoped(typeof(ValidationUtil<>));
        services.AddScoped<IMPosService, MPosService>();
        services.AddHttpClient();
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });
        services.AddHttpContextAccessor();
        services.AddGrpc();
        services.AddGrpcServices(configuration);
        services.AddHealthChecks();
        return services;
    }
    public static IServiceCollection AddGrpcServices(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("GrpcSettings")
            .Get<GrpcSettings>();
        if (settings == null || string.IsNullOrEmpty(settings.StoreUrl))
            throw new ArgumentNullException("Grpc is not configured.");
        
        services.AddGrpcClient<StoreGrpcService.StoreGrpcServiceClient>(x =>
        {
            x.Address = new Uri(settings.StoreUrl);
        });
        
        return services;
    }
}