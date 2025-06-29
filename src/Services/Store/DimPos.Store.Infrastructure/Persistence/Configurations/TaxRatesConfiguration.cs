using DimPos.Store.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Store.Infrastructure.Persistence.Configurations;

public class TaxRatesConfiguration : IEntityTypeConfiguration<TaxRates>
{
    public void Configure(EntityTypeBuilder<TaxRates> builder)
    {
        builder.HasKey(tr => tr.Id);
        builder.Property(tr => tr.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(tr => tr.Rate)
            .IsRequired();
        builder.Property(tr => tr.IsActive)
            .IsRequired();
        builder.Property(tr => tr.BrandId)
            .IsRequired();
        builder.Property(tr => tr.StoreId)
            .IsRequired();
        builder.HasOne(tr => tr.Store)
            .WithMany(s => s.TaxRates)
            .HasForeignKey(tr => tr.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}