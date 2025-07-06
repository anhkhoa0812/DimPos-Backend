using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

public class UnitConversionsConfiguration : IEntityTypeConfiguration<UnitConversions>
{
    public void Configure(EntityTypeBuilder<UnitConversions> builder)
    {
        builder.HasKey(uv => uv.Id);
        builder.Property(uv => uv.FromUnit)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(uv => uv.ToUnit)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(uv => uv.Factor)
            .IsRequired();
        
        builder.HasOne(uv => uv.Ingredient)
            .WithMany(i => i.UnitConversions)
            .HasForeignKey(uv => uv.IngredientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}