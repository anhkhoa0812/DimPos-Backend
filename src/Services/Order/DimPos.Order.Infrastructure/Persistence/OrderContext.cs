using System.Reflection;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Entities.Common.Interface;
using DimPos.Order.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Order.Infrastructure.Persistence;

public class OrderContext : DbContext
{
    public OrderContext(DbContextOptions<OrderContext> options) : base(options)
    {
    }
    
    public virtual DbSet<Orders> Orders { get; set; } = null!;
    public virtual DbSet<OrderItems> OrderItems { get; set; } = null!;
    public virtual DbSet<AppliedOrderPromotions> AppliedOrderPromotions { get; set; } = null!;
    public virtual DbSet<OrderItemSelectedOptions> OrderItemSelectedOptions { get; set; } = null!;
    public virtual DbSet<AppliedTaxes> AppliedTaxes { get; set; } = null!;
    public virtual DbSet<StorePurchaseOrders> StorePurchaseOrders { get; set; } = null!;
    public virtual DbSet<StorePurchaseOrderItems> StorePurchaseOrderItems { get; set; } = null!;
    public virtual DbSet<OrderItemExtras> OrderItemExtras { get; set; } = null!;
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
        return result;
    }
}