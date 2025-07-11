using DimPos.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Order.Infrastructure.Persistence.Configurations;

public class StorePurchaseOrderItemsConfiguration : IEntityTypeConfiguration<StorePurchaseOrderItems>
{
    public void Configure(EntityTypeBuilder<StorePurchaseOrderItems> builder)
    {
        builder.HasKey(spoi => spoi.Id);
        builder.Property(spoi => spoi.StorePurchaseOrderId)
            .IsRequired();
        builder.Property(spoi => spoi.ProductVariantIdSnapshot)
            .IsRequired();
        builder.Property(spoi => spoi.ProductVariantNameSnapshot)
            .IsRequired();
        builder.Property(spoi => spoi.ProductVariantPriceSnapshot)
            .IsRequired();
        builder.Property(spoi => spoi.TotalPriceOfOrderItems)
            .IsRequired();
        builder.Property(spoi => spoi.RequestedQuantity)
            .IsRequired();
        
        builder.HasOne(spoi => spoi.StorePurchaseOrder)
            .WithMany(spo => spo.StorePurchaseOrderItems)
            .HasForeignKey(spoi => spoi.StorePurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}