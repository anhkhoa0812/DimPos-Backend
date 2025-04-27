using DimPos.Catalog.Application.Common.Serilog;
using DimPos.Catalog.Infrastructure;
using DimPos.Catalog.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(SerilogConfiguration.Configure);
Log.Information("Starting Catalog API up");

try
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
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