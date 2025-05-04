using Carter;
using Common.Logging;
using DimPos.Catalog.Application.Common.Extensions;
using DimPos.Catalog.Application.Common.Middlewares;
using DimPos.Catalog.Infrastructure;
using DimPos.Catalog.Infrastructure.Configurations;
using DimPos.Catalog.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Host.UseSerilog(SeriLogger.Configure);
Log.Information("Starting Catalog API up");
try
{
    
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApplicationServices();
    builder.Services.AddCarter(new DependencyContextAssemblyCatalog([typeof(Program).Assembly]));
    // builder.Services.Configure<ApiBehaviorOptions>(options =>
    // {
    //     options.SuppressModelStateInvalidFilter = true;
    // });
    var app = builder.Build();

    if (app.Environment.IsDevelopment() || app.Environment.IsProduction() || app.Environment.IsStaging())
    {
        app.UseScalar();
    }

    app.UseHealthChecks("/health");
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var catalogContextSeed = scope.ServiceProvider.GetRequiredService<CatalogContextSeed>();
            await catalogContextSeed.InitializeAsync();
            // await orderContextSeed.SeedAsync(); 
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while seeding the database.");
            throw; 
        }
    }
    app.UseStaticFiles();
    app.UseMiddleware<GlobalException>();
    app.UseCors(builder =>
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapCarter();
    app.UseHttpsRedirection();
    app.Run();

}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    Log.Fatal(ex, $"Unhandled: {ex.Message}");
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }
    
}
finally
{
    Log.Information("Shut down Catalog API complete");
    Log.CloseAndFlush();
}