using Microsoft.EntityFrameworkCore;

namespace DimPos.Store.Infrastructure.Persistence;

public class StoreContextSeed
{
    private readonly ILogger _logger;
    private readonly StoreContext _context;
    
    public StoreContextSeed(ILogger logger, StoreContext context)
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