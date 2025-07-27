using System.Security.Claims;
using DimPos.Payment.Domain.Settings;
using DimPos.Payment.Infrastructure.Configurations;
using DimPos.Payment.Infrastructure.Persistence;
using DimPos.Payment.Infrastructure.Repositories;
using DimPos.Payment.Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DimPos.Payment.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PaymentContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString"),
                builder => builder.MigrationsAssembly(typeof(PaymentContext).Assembly.FullName));
        });
        services.AddScoped<IUnitOfWork<PaymentContext>, UnitOfWork<PaymentContext>>();
        services.AddScoped<PaymentContextSeed>();
        services.AddJWT(configuration);
        services.AddOpenApiConfig();
        services.AddAuthorization(options =>
            {
                options.AddPolicy("BrandPolicy", policy =>
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "BrandAdmin"));
                options.AddPolicy("StorePolicy", policy =>
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "StoreAdmin", "Staff"));
                options.AddPolicy("SystemAdminPolicy", policy =>
                    policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "SystemAdmin"));
            }
        );        
        services.Configure<MPosSettings>(configuration.GetSection("MPosSettings"));

        services.AddEndpointsApiExplorer();
        services.AddCors();
        return services;
    }
}