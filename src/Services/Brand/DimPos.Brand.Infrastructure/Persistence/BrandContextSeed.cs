using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DimPos.Brand.Infrastructure.Persistence;

public class BrandContextSeed
{
    private readonly ILogger _logger;
    private readonly BrandContext _context;
    
    public BrandContextSeed(ILogger logger, BrandContext context)
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