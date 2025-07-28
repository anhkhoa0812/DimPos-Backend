using DimPos.Inventory.Domain.Entities;
using DimPos.Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Inventory.Infrastructure.Persistence.Configurations;

public class InventoryTransactionsConfiguration : IEntityTypeConfiguration<InventoryTransactions>
{
    public void Configure(EntityTypeBuilder<InventoryTransactions> builder)
    {
        builder.HasKey(it => it.Id);
        builder.Property(it => it.InventoryStockId)
            .IsRequired();
        builder.Property(it => it.Type)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (EInventoryTransactionType)Enum.Parse(typeof(EInventoryTransactionType), v)
            );
        builder.Property(it => it.ReasonManualAdjustment)
            .HasMaxLength(1000);
        builder.Property(it => it.QuantityChange)
            .IsRequired();
        builder.Property(it => it.Note)
            .HasMaxLength(1000);

        builder.HasOne(it => it.InventoryStock)
            .WithMany(i => i.InventoryTransactions)
            .HasForeignKey(it => it.InventoryStockId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}