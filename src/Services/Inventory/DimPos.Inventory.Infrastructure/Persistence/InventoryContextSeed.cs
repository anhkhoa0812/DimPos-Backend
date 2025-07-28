using Microsoft.EntityFrameworkCore;

namespace DimPos.Inventory.Infrastructure.Persistence;

public class InventoryContextSeed
{
    private readonly ILogger _logger;
    private readonly InventoryContext _context;
    
    public InventoryContextSeed(ILogger logger, InventoryContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "An error occurred while migrating the database");
            throw;
        }
    }
}