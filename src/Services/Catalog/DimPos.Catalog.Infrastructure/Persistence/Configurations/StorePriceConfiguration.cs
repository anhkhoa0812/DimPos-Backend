using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the entity framework model for the <c>StorePrice</c> entity.
/// </summary>
/// <remarks>
/// This configuration class sets up the primary key and specific property configurations for the <c>StorePrice</c> entity.
/// </remarks>
public class StorePriceConfiguration : IEntityTypeConfiguration<StorePrice>
{
    public void Configure(EntityTypeBuilder<StorePrice> builder)
    {
        builder.HasKey(sp => sp.Id);
        builder.Property(sp => sp.CurrencyCode)
            .HasMaxLength(10);
        builder.Property(sp => sp.Price)
            .HasPrecision(18,4);
    }
}