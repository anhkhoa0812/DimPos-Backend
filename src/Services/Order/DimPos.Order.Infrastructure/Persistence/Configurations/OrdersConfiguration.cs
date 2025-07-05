using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Order.Infrastructure.Persistence.Configurations;

public class OrdersConfiguration : IEntityTypeConfiguration<Orders>
{
    public void Configure(EntityTypeBuilder<Orders> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.StoreId)
            .IsRequired();
        builder.Property(o => o.BrandId)
            .IsRequired();
        builder.Property(o => o.Type)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (EOrderType)Enum.Parse(typeof(EOrderType), v)
            );
        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (EOrderStatus)Enum.Parse(typeof(EOrderStatus), v)
            );
        builder.Property(c => c.SubTotalAmount)
            .IsRequired();
        builder.Property(c => c.DiscountAmount)
            .IsRequired();
        builder.Property(c => c.TaxAmount)
            .IsRequired();
        builder.Property(c => c.TotalAmount)
            .IsRequired();
        builder.Property(c => c.AmountPaid)
            .IsRequired();
        builder.Property(c => c.CashRoundingAmount)
            .IsRequired();
        builder.Property(o => o.CreatedByAccountId)
            .IsRequired();
        builder.Property(o => o.Note)
            .HasMaxLength(500);
        builder.Property(o => o.SystemPaymentMethodNameSnapshot)
            .IsRequired();
    }
}