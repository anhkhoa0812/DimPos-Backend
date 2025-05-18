using DimPos.Brand.Infrastructure.Configurations;
using DimPos.Brand.Infrastructure.Persistence;
using DimPos.Brand.Infrastructure.Repositories;
using DimPos.Brand.Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DimPos.Brand.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<BrandContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString"),
                builder => builder.MigrationsAssembly(typeof(BrandContext).Assembly.FullName));
        });
        services.AddScoped<IUnitOfWork<BrandContext>, UnitOfWork<BrandContext>>();
        services.AddScoped<BrandContextSeed>();
        services.AddJWT(configuration);
        services.AddOpenApiConfig();
        services.AddAuthorization();
        services.AddAuthentication();
        services.AddEndpointsApiExplorer();
        services.AddCors();
        return services;
    }
}