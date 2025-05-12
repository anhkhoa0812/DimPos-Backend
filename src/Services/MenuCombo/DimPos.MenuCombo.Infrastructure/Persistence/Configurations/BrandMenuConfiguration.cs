using DimPos.MenuCombo.Domain.Entities;
using DimPos.MenuCombo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.MenuCombo.Infrastructure.Persistence.Configurations;

public class BrandMenuConfiguration : IEntityTypeConfiguration<BrandMenu>
{
    public void Configure(EntityTypeBuilder<BrandMenu> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name)
            .HasMaxLength(200);
        builder.Property(m => m.Description)
            .HasMaxLength(1000);
        builder.Property(m => m.Type)
            .HasConversion(
                v => v.ToString(),
                v => (EBrandMenuType)Enum.Parse(typeof(EBrandMenuType), v)
            );
    }
}