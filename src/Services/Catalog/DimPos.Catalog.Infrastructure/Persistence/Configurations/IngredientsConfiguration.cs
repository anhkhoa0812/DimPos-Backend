using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the entity type <see cref="Ingredients"/> for use with Entity Framework Core.
/// </summary>
/// <remarks>
/// This configuration class specifies entity properties, constraints, and database column mappings for the <see cref="Ingredients"/> entity.
/// </remarks>
/// <example>
/// This class is used during the EF Core model building process to define the schema configuration for the <see cref="Ingredients"/> entity.
/// </example>
public class IngredientsConfiguration : IEntityTypeConfiguration<Ingredients>
{
    public void Configure(EntityTypeBuilder<Ingredients> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Code)
            .HasMaxLength(50);
        builder.Property(i => i.Name)
            .HasMaxLength(200);
        builder.Property(i => i.Description)
            .HasMaxLength(1000);
        builder.Property(i => i.MeasureUnit)
            .HasMaxLength(50);
        builder.Property(i => i.Type)
            .HasMaxLength(50);
        builder.Property(i => i.CostPerUnit)
            .HasPrecision(18,4);
    }
}