using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Order.Infrastructure.Persistence.Configurations;

public class StorePurchaseOrdersConfiguration : IEntityTypeConfiguration<StorePurchaseOrders>
{
    public void Configure(EntityTypeBuilder<StorePurchaseOrders> builder)
    {
        builder.HasKey(spo => spo.Id);
        builder.Property(spo => spo.StoreId)
            .IsRequired();
        builder.Property(spo => spo.BrandId)
            .IsRequired();
        builder.Property(spo => spo.Status)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (EStorePurchaseOrderStatus)Enum.Parse(typeof(EStorePurchaseOrderStatus), v)
            );
        builder.Property(spo => spo.CancellationRequestReasonByStore)
            .HasMaxLength(1000);
        builder.Property(spo => spo.CancellationReasonByBrand)
            .HasMaxLength(1000);
        builder.Property(spo => spo.NoteFromStore)
            .HasMaxLength(500);
        builder.Property(spo => spo.NoteFromBrand)
            .HasMaxLength(500);
        builder.Property(spo => spo.CreatedByAccountId)
            .IsRequired();
    }
}