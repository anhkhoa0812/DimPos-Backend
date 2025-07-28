using System.Security.Claims;
using DimPos.Inventory.Infrastructure.Configurations;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.Inventory.Infrastructure.Repositories;
using DimPos.Inventory.Infrastructure.Repositories.Interface;
using DimPos.MenuCombo.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DimPos.Inventory.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<InventoryContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString"),
                builder => builder.MigrationsAssembly(typeof(InventoryContext).Assembly.FullName));
        });
        services.AddScoped<IUnitOfWork<InventoryContext>, UnitOfWork<InventoryContext>>();
        services.AddScoped<InventoryContextSeed>();
        services.AddJWT(configuration);
        services.AddOpenApiConfig();
        services.AddAuthorization(options =>
            {
                options.AddPolicy("BrandPolicy", policy =>
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "BrandAdmin"));
                options.AddPolicy("StorePolicy", policy =>
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "StoreAdmin", "Staff"));
            }
        );        
        services.AddEndpointsApiExplorer();
        services.AddCors();
        return services;
    }
}