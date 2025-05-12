using DimPos.MenuCombo.Domain.Entities;
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
}