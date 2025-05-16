using DimPos.MenuCombo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.MenuCombo.Infrastructure.Persistence.Configurations;

public class BrandMenuItemsConfiguration : IEntityTypeConfiguration<BrandMenuItems>
{
    public void Configure(EntityTypeBuilder<BrandMenuItems> builder)
    {
        builder.HasKey(mi => mi.Id);
        builder.Property(mi => mi.Description)
            .HasMaxLength(255);
        builder.Property(mi => mi.MenuId)
            .IsRequired();
        builder.Property(mi => mi.ProductVariantId)
            .IsRequired();
        builder.HasOne(mi => mi.Menu)
            .WithMany(m => m.MenuItems)
            .HasForeignKey(mi => mi.MenuId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}