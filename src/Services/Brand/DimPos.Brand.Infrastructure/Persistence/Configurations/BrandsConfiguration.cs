using DimPos.Brand.Domain.Entities;
using DimPos.Brand.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Brand.Infrastructure.Persistence.Configurations;

public class BrandsConfiguration: IEntityTypeConfiguration<Brands>
{
    public void Configure(EntityTypeBuilder<Brands> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code)
            .IsUnique();
        builder.Property(x => x.Code)
            .HasMaxLength(50);
        builder.Property(x => x.Name)
            .HasMaxLength(200);
        builder.Property(x => x.Email)
            .HasMaxLength(100);
        builder.Property(x => x.Phone)
            .HasMaxLength(20);
        builder.Property(x => x.Address)
            .HasMaxLength(1000);
        builder.Property(mg => mg.Status)
            .HasConversion(
                v => v.ToString(),
                v => (EBrandStatus)Enum.Parse(typeof(EBrandStatus), v)
            );
    }
}