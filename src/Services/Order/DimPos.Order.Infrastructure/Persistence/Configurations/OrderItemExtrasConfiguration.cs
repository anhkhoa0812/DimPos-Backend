using DimPos.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Order.Infrastructure.Persistence.Configurations;

public class OrderItemExtrasConfiguration : IEntityTypeConfiguration<OrderItemExtras>
{
    public void Configure(EntityTypeBuilder<OrderItemExtras> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(oi => oi.OrderItemId)
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
        builder.HasOne(oi => oi.OrderItem)
            .WithMany(o => o.OrderItemExtras)
            .HasForeignKey(oi => oi.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
        // builder.HasIndex(oi => new { oi.OrderItemId, oi.ProductVariantId })
        //     .IsUnique();
    }
}