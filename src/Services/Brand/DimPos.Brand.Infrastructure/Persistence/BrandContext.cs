using System.Reflection;
using DimPos.Brand.Domain.Entities;
using DimPos.Brand.Domain.Entities.Common.Interface;
using DimPos.Brand.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Brand.Infrastructure.Persistence;

public partial class BrandContext : DbContext
{
    public BrandContext()
    {
    }
    
    public BrandContext(DbContextOptions<BrandContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Brands> Brands { get; set; } = null!;
    public virtual DbSet<BrandAccounts> BrandAccounts { get; set; } = null!;
    public virtual DbSet<FranchiseAgreement> FranchiseAgreement { get; set; } = null!;
    
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