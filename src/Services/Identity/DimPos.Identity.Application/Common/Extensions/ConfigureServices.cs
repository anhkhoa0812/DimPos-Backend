using System.Net.Security;
using System.Net.Sockets;
using DimPos.Brand.Application.Common.Protos;
using DimPos.Identity.Application.Common.Behaviours;
using DimPos.Identity.Application.Common.Config;
using DimPos.Identity.Application.Common.Utils;
using DimPos.Identity.Application.Features.Authentication.Command.Login;
using DimPos.Identity.Application.Services.Implement;
using DimPos.Identity.Application.Services.Interface;
using DimPos.Identity.Domain.Models.Settings;
using DimPos.Store.Application.Common.Protos;
using FluentValidation;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Mediator;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
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
        services.AddGrpcServices(configuration);
        services.AddHealthChecks();
        
        return services;
    }
    public static IServiceCollection AddGrpcServices(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("GrpcSettings")
            .Get<GrpcSettings>();
        if(settings == null || string.IsNullOrEmpty(settings.BrandUrl))
            throw new ArgumentNullException("Grpc is not configured.");

        services.AddGrpcClient<BrandGrpcService.BrandGrpcServiceClient>(
            x =>
            {
                x.Address = new Uri(settings.BrandUrl);
                x.ChannelOptionsActions.Add(channelOptions =>
                {
                    channelOptions.HttpVersion = System.Net.HttpVersion.Version20;
                });
            }
        );
        services.AddGrpcClient<StoreGrpcService.StoreGrpcServiceClient>(
            x =>
            {
                x.Address = new Uri(settings.StoreUrl);
                x.ChannelOptionsActions.Add(channelOptions =>
                {
                    channelOptions.HttpVersion = System.Net.HttpVersion.Version20;
                });
            }
        );
        // services
        //     .AddGrpcClient<BrandGrpcService.BrandGrpcServiceClient>(options =>
        //     {
        //         options.Address = new Uri(settings.BrandUrl);
        //     }).ConfigureChannel(channelOptions =>
        //     {
        //         channelOptions.Credentials = ChannelCredentials.Insecure;
        //         channelOptions.ServiceConfig = new ServiceConfig()
        //         {
        //             LoadBalancingConfigs =
        //             {
        //                 new RoundRobinConfig()
        //             }
        //         };
        //         channelOptions.HttpHandler = new SocketsHttpHandler()
        //         {
        //             EnableMultipleHttp2Connections = true,
        //             SslOptions = new SslClientAuthenticationOptions()
        //             {
        //                 RemoteCertificateValidationCallback = delegate { return true; }
        //             }
        //         };
        //     });
        return services;
    
    }
}