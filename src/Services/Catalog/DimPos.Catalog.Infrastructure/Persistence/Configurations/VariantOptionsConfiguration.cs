using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Represents the Entity Framework Core configuration for the <see cref="VariantOptions"/> entity.
/// </summary>
/// <remarks>
/// This configuration sets up the properties, relationships, and constraints for the
/// VariantOptions table within the database.
/// </remarks>
public class VariantOptionsConfiguration : IEntityTypeConfiguration<VariantOptions>
{
    public void Configure(EntityTypeBuilder<VariantOptions> builder)
    {
        builder.HasKey(vo => vo.Id);
        builder.Property(vo => vo.Name)
            .HasMaxLength(200);
        builder.Property(vo => vo.Description)
            .HasMaxLength(1000);
        builder.Property(vo => vo.Value)
            .HasMaxLength(200);
        builder.HasOne(vo => vo.Variant)
            .WithMany(v => v.VariantOptions)
            .HasForeignKey(vo => vo.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}