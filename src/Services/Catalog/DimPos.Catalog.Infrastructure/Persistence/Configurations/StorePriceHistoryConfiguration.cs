using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Provides the Entity Framework Core configuration for the <see cref="StorePriceHistory"/> entity.
/// This configuration specifies the entity's key, property constraints, and relationships.
/// </summary>
public class StorePriceHistoryConfiguration : IEntityTypeConfiguration<StorePriceHistory>
{
    public void Configure(EntityTypeBuilder<StorePriceHistory> builder)
    {
        builder.HasKey(sph => sph.Id);
        builder.Property(sph => sph.CurrencyCode)
            .HasMaxLength(10);
        builder.Property(sph => sph.OldPrice)
            .HasPrecision(18,4);
        builder.Property(sph => sph.NewPrice)
            .HasPrecision(18,4);
        builder.HasOne(sph => sph.StorePrice)
            .WithMany(sp => sp.StorePriceHistories)
            .HasForeignKey(sph => sph.StorePriceId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}