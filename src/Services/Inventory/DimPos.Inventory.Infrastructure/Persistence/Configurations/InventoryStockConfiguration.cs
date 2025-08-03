using DimPos.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Inventory.Infrastructure.Persistence.Configurations;

public class InventoryStockConfiguration : IEntityTypeConfiguration<InventoryStock>
{
    public void Configure(EntityTypeBuilder<InventoryStock> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.StoreId)
            .IsRequired();
        builder.Property(i => i.IngredientId)
            .IsRequired();
        builder.Property(i => i.Quantity)
            .IsRequired();
    }
}