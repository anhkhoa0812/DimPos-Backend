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
        builder.Property(pv => pv.Description)
            .HasMaxLength(1000);
        builder.Property(pv => pv.Price)
            .IsRequired()
            .HasPrecision(18, 4);
        builder.Property(pv => pv.Sku)
            .HasMaxLength(255);
        builder.Property(pv => pv.IsActive)
            .IsRequired();
        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.ProductVariants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}