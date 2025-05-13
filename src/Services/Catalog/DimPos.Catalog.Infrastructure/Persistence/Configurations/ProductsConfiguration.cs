using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Provides configuration for the <c>Products</c> entity in the Entity Framework.
/// This class defines how the <c>Products</c> entity should be mapped to the underlying database schema.
/// </summary>
public class ProductsConfiguration : IEntityTypeConfiguration<Products>
{
    public void Configure(EntityTypeBuilder<Products> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code)
            .HasMaxLength(50);
        builder.HasIndex(p => p.Code)
            .IsUnique();
        builder.Property(p => p.Name)
            .HasMaxLength(200);
        builder.Property(p => p.Description)
            .HasMaxLength(1000);
        builder.Property(p => p.AlternativeCode)
            .HasMaxLength(100);
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(p => p.Status)
            .HasConversion(
                v => v.ToString(),
                v => (EProductStatus)Enum.Parse(typeof(EProductStatus), v)
            );
    }
}