using System.Reflection;
using Carter;
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
        services.AddSwagger();
        services.Configure<S3CompatibleStorageSettings>(configuration.GetSection("S3CompatibleStorageSettings"));
        services.AddAuthorization();
        services.AddAuthentication();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddCors();
        return services;
    }
}