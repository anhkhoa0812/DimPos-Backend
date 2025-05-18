using System.Security.Claims;
using DimPos.Catalog.Domain.Enums;
using DimPos.Catalog.Domain.Models.Settings;
using DimPos.Catalog.Infrastructure.Configurations;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DimPos.Catalog.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CatalogContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString"),
                builder => builder.MigrationsAssembly(typeof(CatalogContext).Assembly.FullName));
        });
        services.AddScoped<IUnitOfWork<CatalogContext>, UnitOfWork<CatalogContext>>();
        services.AddScoped<CatalogContextSeed>();
        services.AddJWT(configuration); 
        services.AddOpenApiConfig();
        services.Configure<S3CompatibleStorageSettings>(configuration.GetSection("S3CompatibleStorageSettings"));
        services.AddAuthorization(options =>
            {
                options.AddPolicy("BrandPolicy", policy =>
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "BrandAdmin"));
            }
        );
        services.AddEndpointsApiExplorer();
        services.AddCors();
        return services;
    }
}