using DimPos.Payment.Domain.Entities;
using DimPos.Payment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Payment.Infrastructure.Persistence.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransactions>
{
    public void Configure(EntityTypeBuilder<PaymentTransactions> builder)
    {
        builder.HasKey(pt => pt.Id);
        
        builder.Property(pt => pt.Amount)
            .IsRequired()
            .HasPrecision(18,2);
        builder.Property(pt => pt.CurrencyCode)
            .IsRequired()
            .HasMaxLength(10);
        builder.Property(pt => pt.TransactionType)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (ETransactionType)Enum.Parse(typeof(ETransactionType), v)
            );
        builder.Property(pt => pt.TransactionTime)
            .IsRequired();
        builder.HasOne(pt => pt.SystemPaymentMethodType)
            .WithMany(spm => spm.PaymentTransactions)
            .HasForeignKey(pt => pt.SystemPaymentMethodTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}