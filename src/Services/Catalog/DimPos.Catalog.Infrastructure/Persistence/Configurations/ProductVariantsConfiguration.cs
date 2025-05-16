using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the entity properties, relationships, and constraints for the <see cref="ProductVariants"/> entity within the database context.
/// </summary>
/// <remarks>
/// This configuration defines behavior such as primary keys, property precision, unique constraints,
/// foreign key relationships, and cascade delete actions for the <see cref="ProductVariants"/> entity.
/// </remarks>
/// <seealso cref="ProductVariants"/>
/// <seealso cref="IEntityTypeConfiguration{TEntity}"/>
public class ProductVariantsConfiguration : IEntityTypeConfiguration<ProductVariants>
{
    public void Configure(EntityTypeBuilder<ProductVariants> builder)
    {
        builder.HasKey(pv => pv.Id);
        builder.Property(pv => pv.ProductId)
            .IsRequired();
        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasIndex(p => p.Code)
            .IsUnique();
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(p => p.AlternativeCode)
            .HasMaxLength(100);
        builder.Property(pv => pv.Price)
            .IsRequired()
            .HasPrecision(18, 4);
        builder.Property(pv => pv.DiscountPercent)
            .HasPrecision(5, 2);
        builder.Property(pv => pv.DiscountPrice)
            .HasPrecision(18, 4);
        builder.Property(pv => pv.PriceCOGS)
            .HasPrecision(18, 4);
        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.ProductVariants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(pv => pv.Status)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (EProductVariantStatus)Enum.Parse(typeof(EProductVariantStatus), v)
            );
    }
}