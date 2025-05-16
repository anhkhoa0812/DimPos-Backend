using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Provides configuration settings for the <see cref="ProductAttributes"/> entity.
/// </summary>
/// <remarks>
/// This class is responsible for configuring the database schema and relationships
/// for the <see cref="ProductAttributes"/> entity. It defines the primary key,
/// property constraints, and relationships with other entities.
/// </remarks>
public class ProductAttributesConfiguration : IEntityTypeConfiguration<ProductAttributes>
{
    public void Configure(EntityTypeBuilder<ProductAttributes> builder)
    {
        builder.HasKey(pa => pa.Id);
        builder.Property(pa => pa.ProductId)
            .IsRequired();
        builder.Property(pa => pa.Key)
            .HasMaxLength(200);
        builder.Property(pa => pa.Value)
            .HasMaxLength(500);
        builder.HasOne(pa => pa.Product)
            .WithMany(p => p.ProductAttributes)
            .HasForeignKey(pa => pa.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}