using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DimPos.Catalog.Infrastructure.Persistence;

public class CatalogContextSeed
{
    private readonly ILogger _logger;
    private readonly CatalogContext _context;
    
    public CatalogContextSeed(ILogger logger, CatalogContext context)
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