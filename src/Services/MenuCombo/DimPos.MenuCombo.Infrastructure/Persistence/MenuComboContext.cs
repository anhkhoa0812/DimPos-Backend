using DimPos.MenuCombo.Domain.Entities;
using DimPos.MenuCombo.Domain.Entities.Common.Interface;
using DimPos.MenuCombo.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace DimPos.MenuCombo.Infrastructure.Persistence;

public class MenuComboContext : DbContext
{
    public MenuComboContext() {}
    
    public MenuComboContext(DbContextOptions<MenuComboContext> options)
        : base(options)
    {
    }
    
    public virtual DbSet<BrandMenu> Menu { get; set; } = null!;
    public virtual DbSet<BrandMenuItems> MenuItems { get; set; } = null!;
    public virtual DbSet<Collections> Collections { get; set; } = null!;
    public virtual DbSet<CollectionItems> CollectionItems { get; set; } = null!;
    public virtual DbSet<StoreMenuAssignments> StoreMenuAssignments { get; set; } = null!;
    public virtual DbSet<StoreMenuItemAvailability> StoreMenuItemAvailability { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MenuComboContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var modified = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified ||
                            e.State == EntityState.Added ||
                            e.State == EntityState.Deleted);

            foreach (var item in modified)
                switch (item.State)
                {
                    case EntityState.Added:
                        if (item.Entity is IDateTracking addedEntity)
                        {
                            addedEntity.CreatedDate = TimeUtil.GetCurrentSEATime();
                            item.State = EntityState.Added;
                        }

                        break;
                    case EntityState.Modified:
                        if (item.Entity is IDateTracking modifiedEntity)
                        {
                            Entry(item.Entity).Property("Id").IsModified = false;
                            modifiedEntity.LastModifiedDate = TimeUtil.GetCurrentSEATime();
                            item.State = EntityState.Modified;
                        }

                        break;
                }

            var result = await base.SaveChangesAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}