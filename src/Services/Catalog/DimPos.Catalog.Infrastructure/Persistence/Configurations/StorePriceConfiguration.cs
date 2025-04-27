using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

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