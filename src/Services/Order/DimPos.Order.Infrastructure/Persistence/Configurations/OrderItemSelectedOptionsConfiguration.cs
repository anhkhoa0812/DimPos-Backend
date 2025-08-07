using DimPos.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Order.Infrastructure.Persistence.Configurations;

public class OrderItemSelectedOptionsConfiguration : IEntityTypeConfiguration<OrderItemSelectedOptions>
{
    public void Configure(EntityTypeBuilder<OrderItemSelectedOptions> builder)
    {
        builder.HasKey(ois => ois.Id);

        builder.Property(ois => ois.OrderItemId)
            .IsRequired();
        builder.Property(ois => ois.ModifierGroupId)
            .IsRequired();
        builder.Property(ois => ois.ModifierOptionId)
            .IsRequired();
        builder.Property(ois => ois.ModifierGroupSnapshot)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(ois => ois.ModifierOptionSnapshot)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.HasOne(ois => ois.OrderItem)
            .WithMany(oi => oi.OrderItemSelectedOptions)
            .HasForeignKey(ois => ois.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}