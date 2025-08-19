using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

public class ProductExtraItemsConfiguration : IEntityTypeConfiguration<ProductExtraItems>
{
    public void Configure(EntityTypeBuilder<ProductExtraItems> builder)
    {
        builder.HasKey(pci => pci.Id);
        
        builder.Property(pci => pci.ProductId)
            .IsRequired();
        builder.Property(pci => pci.ExtraProductVariantId)
            .IsRequired();
        
        builder.HasOne(pci => pci.Product)
            .WithMany(p => p.ProductExtraItems)
            .HasForeignKey(pci => pci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(pci => pci.ExtraProductVariant)
            .WithMany(pv => pv.ProductExtraItems)
            .HasForeignKey(pci => pci.ExtraProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}