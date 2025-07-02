using DimPos.Payment.Domain.Entities;
using DimPos.Payment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Payment.Infrastructure.Persistence.Configurations;

public class SystemPaymentMethodsConfiguration : IEntityTypeConfiguration<SystemPaymentMethods>
{
    public void Configure(EntityTypeBuilder<SystemPaymentMethods> builder)
    {
        builder.HasKey(spm => spm.Id);
        builder.Property(spm => spm.Code)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(spm => spm.Name)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(spm => spm.Type)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (ESystemPaymentMethod)Enum.Parse(typeof(ESystemPaymentMethod), v)
            );
        builder.Property(spm => spm.IsGloballyActive)
            .IsRequired();
    }
}