using DimPos.Inventory.Domain.Entities;
using DimPos.Inventory.Domain.Entities.Common.Interface;
using DimPos.Inventory.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Inventory.Infrastructure.Persistence;

public class InventoryContext : DbContext
{
    public InventoryContext() {}
    
    public InventoryContext(DbContextOptions<InventoryContext> options)
        : base(options)
    {
    }
    
    public virtual DbSet<InventoryStock> InventoryStock { get; set; }
    public virtual DbSet<InventoryTransactions> InventoryTransactions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryContext).Assembly);
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