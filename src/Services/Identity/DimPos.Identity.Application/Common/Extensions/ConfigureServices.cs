using DimPos.Identity.Application.Common.Config;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace DimPos.Identity.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // services
        //     .AddMediator( options =>
        //     {
        //         options.Namespace = "DimPos.Catalog.Application.Controllers";
        //         options.ServiceLifetime = ServiceLifetime.Scoped;
        //     })
        //     .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>))
        //     .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        // services.AddScoped(typeof(ValidationUtil<>));
        // services.AddScoped<IValidator<CreateProductsCommand>, CreateProductsCommandValidator>();
        // services.AddScoped<IValidator<CreateCategoriesCommand>, CreateCategoriesCommandValidator>();
        // services.AddScoped<IValidator<CreateModifierGroupsCommand>, CreateModifierGroupsCommandValidator>();
        
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });
        services.AddCustomKafka(configuration);
        services.AddOpenApi(options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;

            options.AddDocumentTransformer(async (document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "Identity",
                    Version = "v1",
                    Description = "Identity API",
                    Contact = new OpenApiContact
                    {
                        Name = "DimPos - Ta Hoang Anh Khoa",
                        Email = "tahoanganhkhoa2014@gmail.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    },
                    TermsOfService = new Uri("https://opensource.org/licenses/MIT")
                };
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type         = SecuritySchemeType.Http,
                    Scheme       = "bearer",
                    BearerFormat = "JWT",
                    In           = ParameterLocation.Header,
                    Name         = "Authorization",
                    Description  = "Please enter a valid token using the Bearer scheme (\"Bearer {token}\")"
                };
                document.SecurityRequirements.Add(new OpenApiSecurityRequirement
                {
                    [ new OpenApiSecurityScheme 
                        { Reference = new OpenApiReference 
                            { Type = ReferenceType.SecurityScheme, Id = "Bearer" } 
                        }
                    ] = Array.Empty<string>()
                });
            });
        });
        services.AddHealthChecks();
        return services;
    }
}