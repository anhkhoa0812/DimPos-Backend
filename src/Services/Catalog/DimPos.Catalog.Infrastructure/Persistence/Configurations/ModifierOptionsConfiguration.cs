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
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(mo => mo.Description)
            .HasMaxLength(1000);
        builder.Property(mo => mo.IsActive)
            .IsRequired();
        builder.Property(mo => mo.ModifierGroupId)
            .IsRequired();
        builder.HasOne(mo => mo.ModifierGroup)
            .WithMany(mg => mg.ModifierOptions)
            .HasForeignKey(mo => mo.ModifierGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}