using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Represents the configuration class for the <see cref="BasePrice"/> entity,
/// used to define the entity's schema and mapping configuration for database persistence.
/// </summary>
public class BasePriceConfiguration : IEntityTypeConfiguration<BasePrice>
{
    public void Configure(EntityTypeBuilder<BasePrice> builder)
    {
        builder.HasKey(bp => bp.Id);
        builder.Property(bp => bp.CurrencyCode)
            .HasMaxLength(10);
        builder.Property(bp => bp.Price)
            .HasPrecision(18, 4);
    }
}