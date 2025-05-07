namespace DimPos.Brand.Application.Common.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediator(options => { });
        services.AddHealthChecks();
        return services;
    }
}