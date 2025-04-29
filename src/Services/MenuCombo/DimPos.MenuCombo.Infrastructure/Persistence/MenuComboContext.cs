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
    
    public virtual DbSet<Menu> Menu { get; set; } = null!;
    public virtual DbSet<MenuItems> MenuItems { get; set; } = null!;
    public virtual DbSet<Collections> Collections { get; set; } = null!;
    public virtual DbSet<CollectionItems> CollectionItems { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MenuComboContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}