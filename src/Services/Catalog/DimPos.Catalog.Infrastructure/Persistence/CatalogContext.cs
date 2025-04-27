using System.Reflection;
using DimPos.Catalog.Domain.Entities;
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
    public virtual DbSet<Variants> Variants { get; set; } = null!;
    public virtual DbSet<VariantOptions> VariantOptions { get; set; } = null!;
    public virtual DbSet<Recipes> Recipes { get; set; } = null!;
    public virtual DbSet<RecipeItems> RecipeItems { get; set; } = null!;
    public virtual DbSet<ProductAttributes> ProductAttributes { get; set; } = null!;
    public virtual DbSet<ProductImages> ProductImages { get; set; } = null!;
    public virtual DbSet<BasePrice> BasePrice { get; set; } = null!;
    public virtual DbSet<BrandPriceHistory> BrandPriceHistory { get; set; }
    public virtual DbSet<StorePrice> StorePrice { get; set; } = null!;
    public virtual DbSet<StorePriceHistory> StorePriceHistory { get; set; } = null!;
    public virtual DbSet<ProductVariants> ProductVariants { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}