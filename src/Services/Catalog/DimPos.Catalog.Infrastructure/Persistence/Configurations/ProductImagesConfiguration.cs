using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Provides the entity configuration for the <see cref="ProductImages"/> class in the database context.
/// This configuration defines the primary key, property constraints, and relationships for the entity.
/// </summary>
public class ProductImagesConfiguration : IEntityTypeConfiguration<ProductImages>
{
    public void Configure(EntityTypeBuilder<ProductImages> builder)
    {
        builder.HasKey(pi => pi.Id);
        builder.Property(pi => pi.ImageUrl)
            .HasMaxLength(1000);
        builder.Property(pi => pi.AltText)
            .HasMaxLength(500);
        builder.HasOne(pi => pi.Product)
            .WithMany(p => p.ProductImages)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}