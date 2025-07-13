using System.Security.Claims;
using DimPos.Order.Infrastructure.Configurations;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories;
using DimPos.Order.Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DimPos.Order.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OrderContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString"),
                builder => builder.MigrationsAssembly(typeof(OrderContext).Assembly.FullName));
        });
        services.AddScoped<IUnitOfWork<OrderContext>, UnitOfWork<OrderContext>>();
        services.AddScoped<OrderContextSeed>();
        services.AddJWT(configuration);
        services.AddOpenApiConfig();
        services.AddAuthorization(options =>
            {
                options.AddPolicy("BrandPolicy", policy =>
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "BrandAdmin"));
                options.AddPolicy("StorePolicy", policy => 
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "StoreAdmin"));
                options.AddPolicy("StaffPolicy", policy => 
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "Staff"));
                options.AddPolicy("StoreAndStaffPolicy", policy =>
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "StoreAdmin", "Staff"));
                options.AddPolicy("BrandAndStorePolicy", policy =>
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "BrandAdmin", "StoreAdmin"));
                options.AddPolicy("BrandAndStoreAndStaffPolicy", policy =>
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "BrandAdmin", "StoreAdmin", "Staff"));
            }
        );        
        services.AddEndpointsApiExplorer();
        services.AddCors();
        return services;
    }
}