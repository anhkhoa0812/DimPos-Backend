using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

public class RecipesConfiguration : IEntityTypeConfiguration<Recipes>
{
    public void Configure(EntityTypeBuilder<Recipes> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Version)
            .HasMaxLength(50);
        builder.HasOne(r => r.ProductVariant)
            .WithMany(pv => pv.Recipes)
            .HasForeignKey(r => r.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}