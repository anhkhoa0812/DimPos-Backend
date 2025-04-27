using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

public class ProductAttributesConfiguration : IEntityTypeConfiguration<ProductAttributes>
{
    public void Configure(EntityTypeBuilder<ProductAttributes> builder)
    {
        builder.HasKey(pa => pa.Id);
        builder.Property(pa => pa.Key)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(pa => pa.Value)
            .HasMaxLength(500);
        builder.HasOne(pa => pa.Product)
            .WithMany(p => p.ProductAttributes)
            .HasForeignKey(pa => pa.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}