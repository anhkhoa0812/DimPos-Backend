using DimPos.Payment.Domain.Entities;
using DimPos.Payment.Domain.Entities.Common.Interface;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Payment.Infrastructure.Persistence;

public class PaymentContext : DbContext
{
    public PaymentContext() {}
    
    public PaymentContext(DbContextOptions<PaymentContext> options) : base(options) {}
    
    public virtual DbSet<SystemPaymentMethods> SystemPaymentMethodTypes { get; set; }
    public virtual DbSet<PaymentTransactions> PaymentTransactions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentContext).Assembly);
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