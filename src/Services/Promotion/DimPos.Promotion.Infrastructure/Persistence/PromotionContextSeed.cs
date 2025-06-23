using Microsoft.EntityFrameworkCore;

namespace DimPos.Promotion.Infrastructure.Persistence;

public class PromotionContextSeed
{
    private readonly ILogger _logger;
    private readonly PromotionContext _context;
    
    public PromotionContextSeed(ILogger logger, PromotionContext context)
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