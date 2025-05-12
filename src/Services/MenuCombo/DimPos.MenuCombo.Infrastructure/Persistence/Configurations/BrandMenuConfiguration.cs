using DimPos.MenuCombo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.MenuCombo.Infrastructure.Persistence.Configurations;

public class BrandMenuConfiguration : IEntityTypeConfiguration<BrandMenu>
{
    public void Configure(EntityTypeBuilder<BrandMenu> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name)
            .HasMaxLength(100);
        builder.Property(m => m.Description)
            .HasMaxLength(255);
        builder.Property(m => m.Type)
            .HasMaxLength(50);
    }
}