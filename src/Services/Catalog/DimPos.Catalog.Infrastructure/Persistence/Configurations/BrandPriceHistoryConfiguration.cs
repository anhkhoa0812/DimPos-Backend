using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Provides the Entity Framework Core configuration for the <see cref="BrandPriceHistory"/> entity.
/// </summary>
/// <remarks>
/// Configures the primary key, property constraints, and relationships for the <see cref="BrandPriceHistory"/> entity.
/// </remarks>
public class BrandPriceHistoryConfiguration : IEntityTypeConfiguration<BrandPriceHistory>
{
    public void Configure(EntityTypeBuilder<BrandPriceHistory> builder)
    {
        builder.HasKey(bph => bph.Id);
        builder.Property(bph => bph.CurrencyCode)
            .HasMaxLength(10);
        builder.Property(bph => bph.OldPrice)
            .HasPrecision(18,4);
        builder.Property(bph => bph.NewPrice)
            .HasPrecision(18,4);
        builder.HasOne(bph => bph.BrandPrice)
            .WithMany(bp => bp.BrandPriceHistories)
            .HasForeignKey(bph => bph.BrandPriceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}