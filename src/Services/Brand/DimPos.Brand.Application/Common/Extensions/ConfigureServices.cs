using DimPos.Brand.Application.Common.Behaviours;
using DimPos.Brand.Application.Common.Protos;
using DimPos.Brand.Application.Common.Utils;
using DimPos.Brand.Application.Features.Brands.Command.CreateBrand;
using DimPos.Brand.Application.Features.Brands.Command.UpdateBrands;
using DimPos.Brand.Application.Features.Brands.Command.UpdateBrandsById;
using DimPos.Brand.Application.Features.Brands.Command.UpdatePassword;
using DimPos.Brand.Application.Services.Implement;
using DimPos.Brand.Application.Services.Interface;
using DimPos.Brand.Domain.Models.Settings;
using DimPos.Media.Application.Common.Protos;
using FluentValidation;
using Mediator;

namespace DimPos.Brand.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
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
        services.AddScoped<IValidator<UpdatePasswordCommand>, UpdatePasswordCommandValidator>();
        services.AddScoped<IValidator<UpdateBrandsCommand>, UpdateBrandsCommandValidator>();
        services.AddScoped<IValidator<UpdateBrandsByIdCommand>, UpdateBrandsByIdCommandValidator>();
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });
        services.AddCustomKafka(configuration);
        services.AddGrpc();
        services.AddGrpcServices(configuration);
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
        });
        
        return services;
    }

}