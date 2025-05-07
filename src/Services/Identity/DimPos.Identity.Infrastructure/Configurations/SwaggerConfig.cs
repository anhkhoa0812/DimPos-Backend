using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace DimPos.Identity.Infrastructure.Configurations;

public static class SwaggerConfig
{
    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(swagger =>
        {
            swagger.SwaggerDoc("v1", new()
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
            });
            swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token using the Bearer scheme (\"bearer {token}\")",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "Security",
                Scheme = "Bearer"
            });
            swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
            });
        });
    }
}