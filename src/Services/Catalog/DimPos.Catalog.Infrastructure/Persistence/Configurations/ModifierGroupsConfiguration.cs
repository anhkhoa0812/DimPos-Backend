using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

public class ModifierGroupsConfiguration : IEntityTypeConfiguration<ModifierGroups>
{
    public void Configure(EntityTypeBuilder<ModifierGroups> builder)
    {
        builder.HasKey(mg => mg.Id);
        builder.Property(mg => mg.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(mg => mg.Description)
            .HasMaxLength(1000);
        builder.Property(mg => mg.SelectedType)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (ESelectedTypeModifier)Enum.Parse(typeof(ESelectedTypeModifier), v)
            );
        builder.Property(mg => mg.Status)
            .IsRequired();
        builder.Property(mg => mg.BrandId)
            .IsRequired();
    }
}