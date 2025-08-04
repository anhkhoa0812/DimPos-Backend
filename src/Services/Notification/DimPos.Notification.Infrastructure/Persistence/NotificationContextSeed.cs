using Microsoft.EntityFrameworkCore;

namespace DimPos.Notification.Infrastructure.Persistence;

public class NotificationContextSeed
{
    private readonly ILogger _logger;
    private readonly NotificationContext _context;
    
    public NotificationContextSeed(ILogger logger, NotificationContext context)
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