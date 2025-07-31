using Common.Logging;
using DimPos.Orchestrator.Common.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(SeriLogger.Configure);
Log.Information("Starting Orchestrator up");
try
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddCustomKafka(builder.Configuration);
    var app = builder.Build();

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
    Log.Information("Shut down Orchestrator complete");
    Log.CloseAndFlush();
}

