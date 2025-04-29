using DimPos.MenuCombo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.MenuCombo.Infrastructure.Persistence.Configurations;

public class MenuItemsConfiguration : IEntityTypeConfiguration<MenuItems>
{
    public void Configure(EntityTypeBuilder<MenuItems> builder)
    {
        builder.HasKey(mi => mi.Id);
        builder.Property(mi => mi.Description)
            .HasMaxLength(255);
        builder.Property(mi => mi.ItemType)
            .HasMaxLength(50);
        builder.HasOne(mi => mi.Menu)
            .WithMany(m => m.MenuItems)
            .HasForeignKey(mi => mi.MenuId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}