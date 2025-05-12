using Microsoft.EntityFrameworkCore;

namespace DimPos.MenuCombo.Infrastructure.Persistence;

public class MenuComboContextSeed
{
    private readonly ILogger _logger;
    private readonly MenuComboContext _context;
    
    public MenuComboContextSeed(ILogger logger, MenuComboContext context)
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