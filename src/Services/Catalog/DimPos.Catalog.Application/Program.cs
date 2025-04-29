using Common.Logging;
using DimPos.Catalog.Application.Common.Behaviours;
using DimPos.Catalog.Application.Common.Extensions;
using DimPos.Catalog.Application.Common.Middlewares;
using DimPos.Catalog.Infrastructure;
using DimPos.Catalog.Infrastructure.Configurations;
using DimPos.Catalog.Infrastructure.Persistence;
using Mediator;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(SeriLogger.Configure);
Log.Information("Starting Catalog API up");

try
{
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApplicationServices();
    var app = builder.Build();

    if (app.Environment.IsDevelopment() || app.Environment.IsProduction() || app.Environment.IsStaging())
    {
        app.UseScalar();
    }
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var orderContextSeed = scope.ServiceProvider.GetRequiredService<CatalogContextSeed>();
            await orderContextSeed.InitializeAsync();
            // await orderContextSeed.SeedAsync(); 
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while seeding the database.");
            throw; 
        }
    }

    app.UseMiddleware<GlobalException>();
    app.UseCors(builder =>
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("X-Pagination"));
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.UseHttpsRedirection();
    app.Run();

}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }

    Log.Fatal(ex, $"Unhandled: {ex.Message}");
}
finally
{
    Log.Information("Shut down Catalog API complete");
    Log.CloseAndFlush();
}