using System.Reflection;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Entities.Common.Interface;
using DimPos.Catalog.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Catalog.Infrastructure.Persistence;

public partial class CatalogContext : DbContext
{
    public CatalogContext()
    {
    }
    
    public CatalogContext(DbContextOptions<CatalogContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categories> Categories { get; set; } = null!;
    public virtual DbSet<Products> Products { get; set; } = null!;
    public virtual DbSet<Ingredients> Ingredients { get; set; } = null!;
    public virtual DbSet<RecipeItems> RecipeItems { get; set; } = null!;
    public virtual DbSet<ProductAttributes> ProductAttributes { get; set; } = null!;
    public virtual DbSet<ProductImages> ProductImages { get; set; } = null!;
    public virtual DbSet<BasePrice> BasePrice { get; set; } = null!;
    public virtual DbSet<BrandPriceHistory> BrandPriceHistory { get; set; }
    public virtual DbSet<StorePrice> StorePrice { get; set; } = null!;
    public virtual DbSet<StorePriceHistory> StorePriceHistory { get; set; } = null!;
    public virtual DbSet<ProductVariants> ProductVariants { get; set; } = null!;
    public virtual DbSet<ProductModifierGroups> ProductModifierGroups { get; set; } = null!;
    public virtual DbSet<ModifierGroups> ModifierGroups { get; set; } = null!;
    public virtual DbSet<ModifierOptions> ModifierOptions { get; set; } = null!;
    public virtual DbSet<UnitConversions> UnitConversions { get; set; } = null!;
    public virtual DbSet<ProductComboItems> ProductComboItems { get; set; } = null!;
    public virtual DbSet<ProductExtraItems> ProductExtraItems { get; set; } = null!;
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