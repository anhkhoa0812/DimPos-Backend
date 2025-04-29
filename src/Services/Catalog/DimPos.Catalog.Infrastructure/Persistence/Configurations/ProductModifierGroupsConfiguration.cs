using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

public class ProductModifierGroupsConfiguration : IEntityTypeConfiguration<ProductModifierGroups>
{
    public void Configure(EntityTypeBuilder<ProductModifierGroups> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne(pmg => pmg.Product)
            .WithMany(p => p.ProductModifierGroups)
            .HasForeignKey(pmg => pmg.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(pmg => pmg.ModifierGroup)
            .WithMany(mg => mg.ProductModifierGroups)
            .HasForeignKey(pmg => pmg.ModifierGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}