using DimPos.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Order.Infrastructure.Persistence.Configurations;

public class OrderItemsConfiguration : IEntityTypeConfiguration<OrderItems>
{
    public void Configure(EntityTypeBuilder<OrderItems> builder)
    {
        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.OrderId)
            .IsRequired();
        builder.Property(oi => oi.ProductVariantId)
            .IsRequired();
        builder.Property(oi => oi.ProductNameSnapshot)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(oi => oi.ProductVariantNameSnapshot)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(oi => oi.Quantity)
            .IsRequired();
        builder.Property(oi => oi.UnitPriceSnapshot)
            .IsRequired();
        builder.Property(oi => oi.TotalPriceBeforeItemDiscount)
            .IsRequired();
        builder.Property(oi => oi.ItemDiscountAmount)
            .IsRequired();
        builder.Property(oi => oi.FinalPrice)
            .IsRequired();
        builder.Property(oi => oi.Note)
            .HasMaxLength(500);
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}