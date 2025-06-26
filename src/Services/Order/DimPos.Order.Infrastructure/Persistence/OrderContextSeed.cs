using Microsoft.EntityFrameworkCore;

namespace DimPos.Order.Infrastructure.Persistence;

public class OrderContextSeed
{
    private readonly ILogger _logger;
    private readonly OrderContext _context;
    
    public OrderContextSeed(ILogger logger, OrderContext context)
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