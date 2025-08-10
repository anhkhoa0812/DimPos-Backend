using System.Security.Claims;
using DimPos.MenuCombo.Infrastructure.Configurations;
using DimPos.MenuCombo.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Repositories;
using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
using DimPos.Store.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DimPos.MenuCombo.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<MenuComboContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString"),
                builder => builder.MigrationsAssembly(typeof(MenuComboContext).Assembly.FullName));
        });
        services.AddScoped<IUnitOfWork<MenuComboContext>, UnitOfWork<MenuComboContext>>();
        services.AddScoped<MenuComboContextSeed>();
        services.AddJWT(configuration);
        services.AddOpenApiConfig();
        services.AddAuthorization(options =>
            {
                options.AddPolicy("BrandPolicy", policy =>
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "BrandAdmin"));
                options.AddPolicy("StorePolicy", policy =>
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "StoreAdmin", "Staff"));
                options.AddPolicy("BrandAndStorePolicy", policy =>
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "BrandAdmin", "StoreAdmin", "Staff"));
            }
        );        
        services.AddEndpointsApiExplorer();
        services.AddCors();
        return services;
    }
}