using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the entity schema and relationships for the <see cref="Variants"/> entity.
/// </summary>
public class VariantsConfiguration : IEntityTypeConfiguration<Variants>
{
    public void Configure(EntityTypeBuilder<Variants> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Name)
            .HasMaxLength(200);
        builder.Property(v => v.Description)
            .HasMaxLength(1000);
    }
}