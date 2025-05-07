using Common.Logging;
using DimPos.Identity.Application.Common.Extensions;
using DimPos.Identity.Infrastructure;
using DimPos.Identity.Infrastructure.Configurations;
using DimPos.Identity.Infrastructure.Persistence;
using Serilog;


var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Host.UseSerilog(SeriLogger.Configure);
Log.Information("Starting Identity API up");
try
{
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApplicationServices(builder.Configuration);
    var app = builder.Build();

    if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
    {
        app.UseScalar();
    }

    app.UseHealthChecks("/health");
    
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var identityContextSeed = scope.ServiceProvider.GetRequiredService<IdentityContextSeed>();
            await identityContextSeed.InitializeAsync();
            await identityContextSeed.SeedAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while seeding the database.");
            throw; 
        }
    }

    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors(builder =>
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
    app.UseAuthentication();
    app.UseAuthorization();
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
    Log.Information("Shut down Identity API complete");
    Log.CloseAndFlush();
}
