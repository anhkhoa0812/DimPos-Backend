using DimPos.Identity.Domain.Entities;
using DimPos.Identity.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DimPos.Identity.Infrastructure.Persistence;

public class IdentityContextSeed
{
    private readonly ILogger _logger;
    private readonly IdentityContext _context;
    
    public IdentityContextSeed(ILogger logger, IdentityContext context)
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
    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.Error(e, "An error occurred while seeding the database");
            throw;
        }
    }
    public async Task TrySeedAsync()
    {
        if (!_context.Roles.Any())
        {
            await _context.Roles.AddRangeAsync(
                new Role()
                {
                    Name = ERoleName.SystemAdmin,
                    Id = Guid.Parse("2cd9da9a-471b-46a4-b264-a5de9516f286"),
                    ShortName = "SYSA"
                },
                new Role()
                {
                    Name = ERoleName.BrandAdmin,
                    Id = Guid.Parse("e19907f5-9866-4fd5-a7a9-06889af49fa5"),
                    ShortName = "BA"
                },
                new Role()
                {
                    Name = ERoleName.StoreAdmin,
                    Id = Guid.Parse("fe7a18f0-22e7-4cbd-9f85-aba96965a2e2"),
                    ShortName = "SA"
                },
                new Role()
                {
                    Name = ERoleName.Staff,
                    Id = Guid.Parse("b954d3b7-4725-468d-9696-efff7f647686"),
                    ShortName = "S"
                },
                new Role()
                {
                    Name = ERoleName.Customer,
                    Id = Guid.Parse("eba956b1-57d5-44e1-b24a-ac9b28db8cb6"),
                    ShortName = "C"
                }
            );
        }
    }
}