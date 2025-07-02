using DimPos.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Order.Infrastructure.Persistence.Configurations;

public class AppliedTaxesConfiguration : IEntityTypeConfiguration<AppliedTaxes>
{
    public void Configure(EntityTypeBuilder<AppliedTaxes> builder)
    {
        builder.HasKey(at => at.Id);
        builder.Property(at => at.OrderId)
            .IsRequired();
        builder.Property(at => at.TaxRateId)
            .IsRequired();
        builder.Property(at => at.TaxNameSnapshot)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(at => at.TaxRateSnapshot)
            .IsRequired();
        builder.HasOne(at => at.Order)
            .WithOne(at => at.AppliedTax)
            .HasForeignKey<AppliedTaxes>(at => at.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}