using Carter;
using Common.Logging;
using DimPos.Inventory.Application.Common.Extensions;
using DimPos.Inventory.Application.Common.Middlewares;
using DimPos.Inventory.Infrastructure;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.MenuCombo.Infrastructure.Configurations;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Host.UseSerilog(SeriLogger.Configure);
Log.Information("Starting Inventory API up");
try
{
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ConfigureHttpsDefaults(httpsOptions =>
        {
            httpsOptions.AllowAnyClientCertificate();
        });
    });
    builder.Services.AddCarter(new DependencyContextAssemblyCatalog([typeof(Program).Assembly]));
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
            var inventoryContextSeed = scope.ServiceProvider.GetRequiredService<InventoryContextSeed>();
            await inventoryContextSeed.InitializeAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while seeding the database.");
            throw; 
        }
    }
    app.UseRouting();
    app.UseStaticFiles();
    app.UseCors(builder =>
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
    app.UseMiddleware<GlobalException>();
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
    Log.Information("Shut down Inventory API complete");
    Log.CloseAndFlush();
}