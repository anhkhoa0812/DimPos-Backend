using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

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