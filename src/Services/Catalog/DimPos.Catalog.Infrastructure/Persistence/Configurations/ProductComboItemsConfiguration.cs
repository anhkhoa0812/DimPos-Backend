using DimPos.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Catalog.Infrastructure.Persistence.Configurations;

public class ProductComboItemsConfiguration : IEntityTypeConfiguration<ProductComboItems>
{
    public void Configure(EntityTypeBuilder<ProductComboItems> builder)
    {
        builder.HasKey(pci => pci.Id);
        
        builder.Property(pci => pci.Quantity)
            .IsRequired();
        
        builder.HasOne(pci => pci.Product)
            .WithMany(p => p.ProductComboItems)
            .HasForeignKey(pci => pci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(pci => pci.ItemProductVariant)
            .WithMany(pv => pv.ProductComboItems)
            .HasForeignKey(pci => pci.ItemProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}