using System.Security.Claims;
using DimPos.Promotion.Infrastructure.Configurations;
using DimPos.Promotion.Infrastructure.Persistence;
using DimPos.Promotion.Infrastructure.Repositories;
using DimPos.Promotion.Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DimPos.Promotion.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PromotionContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString"),
                builder => builder.MigrationsAssembly(typeof(PromotionContext).Assembly.FullName));
        });
        services.AddScoped<IUnitOfWork<PromotionContext>, UnitOfWork<PromotionContext>>();
        services.AddJWT(configuration);
        services.AddOpenApiConfig();
        services.AddAuthorization(options =>
            {
                options.AddPolicy("BrandPolicy", policy =>
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "BrandAdmin"));
                options.AddPolicy("StorePolicy", policy => 
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "StoreAdmin"));
            }
        );        
        services.AddEndpointsApiExplorer();
        services.AddCors();
        return services;
    }
}