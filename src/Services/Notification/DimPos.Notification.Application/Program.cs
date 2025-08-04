using Carter;
using Common.Logging;
using DimPos.Notification.Application.Common.Extensions;
using DimPos.Notification.Infrastructure;
using DimPos.Notification.Infrastructure.Configurations;
using DimPos.Notification.Infrastructure.Persistence;
using DimPos.Notification.Application.Common.Middlewares;
using DimPos.Notification.Application.SignalR;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Host.UseSerilog(SeriLogger.Configure);
Log.Information("Starting Notification API up");
try
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", builder =>
        {
            builder.AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetIsOriginAllowed(_ => true);
        });
    });
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
            var notificationContextSeed = scope.ServiceProvider.GetRequiredService<NotificationContextSeed>();
            await notificationContextSeed.InitializeAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while seeding the database.");
            throw; 
        }
    }
    app.UseRouting();
    app.UseStaticFiles();
    app.UseMiddleware<GlobalException>();
    app.UseCors("CorsPolicy");
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapCarter();
    app.MapHub<NotificationHub>("/hubs/notification");
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
    Log.Information("Shut down Notification API complete");
    Log.CloseAndFlush();
}