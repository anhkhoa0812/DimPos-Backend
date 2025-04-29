using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

public class ModifierGroupsConfiguration : IEntityTypeConfiguration<ModifierGroups>
{
    public void Configure(EntityTypeBuilder<ModifierGroups> builder)
    {
        builder.HasKey(mg => mg.Id);
        builder.Property(mg => mg.Name)
            .HasMaxLength(200);
        builder.Property(mg => mg.Description)
            .HasMaxLength(1000);
    }
}