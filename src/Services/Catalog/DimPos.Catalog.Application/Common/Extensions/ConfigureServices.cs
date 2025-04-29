using DimPos.Catalog.Application.Common.Behaviours;
using DimPos.Catalog.Application.Services.Implement;
using DimPos.Catalog.Application.Services.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services
            .AddMediator( options =>
            {
                options.Namespace = "DimPos.Catalog.Application.Controllers";
                options.ServiceLifetime = ServiceLifetime.Scoped;
            })
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddScoped<IUploadService, UploadService>();
        return services;
    }
}