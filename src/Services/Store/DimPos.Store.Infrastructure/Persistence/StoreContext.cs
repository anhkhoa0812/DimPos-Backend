using System.Reflection;
using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Entities.Common.Interface;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Store.Infrastructure.Persistence;

public class StoreContext : DbContext
{
    public StoreContext(DbContextOptions<StoreContext> options) : base(options)
    {
    }
    public StoreContext() {}

    public virtual DbSet<Domain.Entities.Store> Store { get; set; } = null!;
    public virtual DbSet<StoreAccounts> StoreAccounts { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
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
                        addedEntity.CreatedDate = DateTime.UtcNow;
                        item.State = EntityState.Added;
                    }

                    break;
                case EntityState.Modified:
                    if (item.Entity is IDateTracking modifiedEntity)
                    {
                        Entry(item.Entity).Property("Id").IsModified = false;
                        modifiedEntity.LastModifiedDate = DateTime.UtcNow;
                        item.State = EntityState.Modified;
                    }

                    break;
            }

        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }
}