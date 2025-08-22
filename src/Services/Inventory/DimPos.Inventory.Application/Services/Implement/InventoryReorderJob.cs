using DimPos.Inventory.Application.Services.Interface;
using DimPos.Inventory.Infrastructure.Utils;
using Hangfire;

namespace DimPos.Inventory.Application.Services.Implement;

public class InventoryReorderJob
{
    private readonly IHangfireService _hangfireService;
    private readonly ILogger _logger;

    public InventoryReorderJob(
        IHangfireService hangfireService,
        ILogger logger)
    {
        _hangfireService = hangfireService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync()
    {
        _logger.Information("Starting inventory reorder level check job at {Time}", TimeUtil.GetCurrentSEATime());
        await _hangfireService.CheckReOrderLevelAsync();
        _logger.Information("Completed inventory reorder level check job at {Time}", TimeUtil.GetCurrentSEATime());
    }
}