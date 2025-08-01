using Carter;
using Common.Logging;
using DimPos.Order.Application.Common.Middlewares;
using DimPos.Order.Infrastructure;
using DimPos.Order.Infrastructure.Configurations;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Application.Common.Extensions;
using DimPos.Order.Application.GrpcServices;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Host.UseSerilog(SeriLogger.Configure);
Log.Information("Starting Order API up");

try
{
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddCarter(new DependencyContextAssemblyCatalog([typeof(Program).Assembly]));
    var app = builder.Build();
    
    if (app.Environment.IsDevelopment() || app.Environment.IsProduction() || app.Environment.IsStaging())
    {
        app.UseScalar();
    }
    
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var orderContextSeed = scope.ServiceProvider.GetRequiredService<OrderContextSeed>();
            await orderContextSeed.InitializeAsync();
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
    app.MapGrpcService<OrderGrpcService>();
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
    Log.Information("Shut down Order API complete");
    Log.CloseAndFlush();
}