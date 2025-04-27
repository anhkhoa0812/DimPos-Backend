using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

public class ProductVariantsConfiguration : IEntityTypeConfiguration<ProductVariants>
{
    public void Configure(EntityTypeBuilder<ProductVariants> builder)
    {
        builder.HasKey(pv => pv.Id);
        builder.Property(pv => pv.Price)
            .HasPrecision(18, 4);
        builder.Property(pv => pv.DiscountPercent)
            .HasPrecision(5, 2);
        builder.Property(pv => pv.DiscountPrice)
            .HasPrecision(18, 4);
        builder.HasIndex(pv => pv.ProductId)
            .IsUnique();
        builder.HasIndex(pv => pv.VariantOptionId)
            .IsUnique();
        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.ProductVariants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(pv => pv.VariantOption)
            .WithMany(vo => vo.ProductVariants)
            .HasForeignKey(pv => pv.VariantOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}