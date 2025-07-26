using Carter;
using Common.Logging;
using DimPos.Payment.Application.Common.Extensions;
using DimPos.Payment.Application.Common.Middlewares;
using DimPos.Payment.Application.GrpcServices;
using DimPos.Payment.Infrastructure;
using DimPos.Payment.Infrastructure.Configurations;
using DimPos.Payment.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(SeriLogger.Configure);
Log.Information("Starting Payment API up");
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
            var paymentContextSeed = scope.ServiceProvider.GetRequiredService<PaymentContextSeed>();
            await paymentContextSeed.InitializeAsync();
            await paymentContextSeed.SeedAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while seeding the database.");
            throw; 
        }
    }
    app.UseRouting();
    app.UseDefaultFiles();
    app.UseStaticFiles(options: new StaticFileOptions()
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append(
                "Access-Control-Allow-Origin", "*"
            );
        }
    });
    app.MapGrpcService<PaymentGrpcService>();
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
    Log.Information("Shut down Payment API complete");
    Log.CloseAndFlush();
}
