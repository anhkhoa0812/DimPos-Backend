using DimPos.Identity.Domain.Models.Settings;
using DimPos.Identity.Infrastructure.Configurations;
using DimPos.Identity.Infrastructure.Persistence;
using DimPos.Identity.Infrastructure.Repositories;
using DimPos.Identity.Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DimPos.Identity.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<IdentityContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString"),
                builder => builder.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName));
        });
        services.AddScoped<IUnitOfWork<IdentityContext>, UnitOfWork<IdentityContext>>();
        services.AddScoped<IdentityContextSeed>();
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddJwt(configuration);
        services.AddOpenApiConfig();
        services.AddAuthorization();
        services.AddAuthentication();
        services.AddEndpointsApiExplorer();
        services.AddCors();
        return services;
    }
}