using DimPos.MenuCombo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.MenuCombo.Infrastructure.Persistence.Configurations;

public class CollectionItemsConfiguration : IEntityTypeConfiguration<CollectionItems>
{
    public void Configure(EntityTypeBuilder<CollectionItems> builder)
    {
        builder.HasKey(ci => ci.Id);
        builder.Property(ci => ci.ProductVariantId)
            .IsRequired();
        builder.Property(ci => ci.CollectionId)
            .IsRequired();
        builder.HasOne(ci => ci.Collection)
            .WithMany(c => c.CollectionItems)
            .HasForeignKey(ci => ci.CollectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}