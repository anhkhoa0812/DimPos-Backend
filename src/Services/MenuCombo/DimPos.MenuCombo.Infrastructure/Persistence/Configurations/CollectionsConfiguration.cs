using DimPos.MenuCombo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.MenuCombo.Infrastructure.Persistence.Configurations;

public class CollectionsConfiguration : IEntityTypeConfiguration<Collections>
{
    public void Configure(EntityTypeBuilder<Collections> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name)
            .HasMaxLength(100);
        builder.Property(c => c.Description)
            .HasMaxLength(255);
        builder.Property(c => c.SKU)
            .HasMaxLength(50);
    }
}