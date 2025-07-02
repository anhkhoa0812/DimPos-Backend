using DimPos.Store.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Store.Infrastructure.Persistence.Configurations;

public class StorePaymentMethodConfigsConfiguration : IEntityTypeConfiguration<StorePaymentMethodConfigs>
{
    public void Configure(EntityTypeBuilder<StorePaymentMethodConfigs> builder)
    {
        builder.HasKey(spmc => spmc.Id);

        builder.Property(spmc => spmc.StoreId)
            .IsRequired();
        builder.Property(spmc => spmc.IsActiveByStore)
            .IsRequired();
        builder.HasOne(spmc => spmc.Store)
            .WithMany(s => s.StorePaymentMethodConfigs)
            .HasForeignKey(spmc => spmc.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}