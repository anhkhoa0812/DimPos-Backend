using System.Security.Claims;
using Carter;
using Common.Logging;
using DimPos.Basket.Application.Common.Extensions;
using DimPos.Basket.Application.Common.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// builder.AddServiceDefaults();
builder.Host.UseSerilog(SeriLogger.Configure);
try
{
    Log.Information("Starting Baskets API up");
    builder.Services.AddCarter(new DependencyContextAssemblyCatalog([typeof(Program).Assembly]));
    builder.Services.AddServices();
    builder.Services.AddRedis(configuration: builder.Configuration);
    builder.Services.AddJWT(configuration: builder.Configuration);
    builder.Services.AddOpenApiConfig();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("BrandPolicy", policy =>
                policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "BrandAdmin"));
            options.AddPolicy("StaffPolicy", policy => 
                policy.RequireAuthenticatedUser().RequireRole(ClaimTypes.Role, "Staff"));
        }
    );
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddCors();
    var app = builder.Build();

    if (app.Environment.IsDevelopment() || app.Environment.IsProduction() || app.Environment.IsStaging())
    {
        app.UseScalar();
    }

    app.UseRouting();
    app.UseStaticFiles();
    app.MapCarter();
    app.UseMiddleware<GlobalException>();
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
    Log.Information("Shut down Baskets API complete");
    Log.CloseAndFlush();
}
