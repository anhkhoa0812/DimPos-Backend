using DimPos.Store.Infrastructure.Configurations;
using DimPos.Store.Infrastructure.Persistence;
using DimPos.Store.Infrastructure.Repositories;
using DimPos.Store.Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DimPos.Store.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<StoreContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString"),
                builder => builder.MigrationsAssembly(typeof(StoreContext).Assembly.FullName));
        });
        services.AddScoped<IUnitOfWork<StoreContext>, UnitOfWork<StoreContext>>();
        services.AddScoped<StoreContextSeed>();
        services.AddCustomKafka(configuration);
        services.AddJWT(configuration);
        services.AddOpenApiConfig();
        services.AddAuthorization();
        services.AddAuthentication();
        services.AddEndpointsApiExplorer();
        services.AddCors();
        return services;
    }
}