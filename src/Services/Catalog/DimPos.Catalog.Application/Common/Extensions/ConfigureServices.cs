using DimPos.Catalog.Application.Common.Behaviours;
using DimPos.Catalog.Application.Common.Utils;
using DimPos.Catalog.Application.Features.Categories;
using DimPos.Catalog.Application.Features.ModifierGroups.Command.CreateModifierGroups;
using DimPos.Catalog.Application.Features.Products.Commands.CreateProducts;
using DimPos.Catalog.Application.Services.Implement;
using DimPos.Catalog.Application.Services.Interface;
using FluentValidation;
using Mediator;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace DimPos.Catalog.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
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
        services.AddHttpContextAccessor();
        services.AddScoped<IUploadService, UploadService>();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddHealthChecks();
        return services;
    }
}