using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

public class ModifierOptionsConfiguration : IEntityTypeConfiguration<ModifierOptions>
{
    public void Configure(EntityTypeBuilder<ModifierOptions> builder)
    {
        builder.HasKey(mo => mo.Id);
        builder.Property(mo => mo.Name)
            .HasMaxLength(200);
        builder.Property(mo => mo.Description)
            .HasMaxLength(1000);
        builder.Property(mo => mo.PriceDelta)
            .HasPrecision(18, 4);
        builder.HasOne(mo => mo.ModifierGroup)
            .WithMany(mg => mg.ModifierOptions)
            .HasForeignKey(mo => mo.ModifierGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}