using DimPos.Store.Domain.Entities;
using DimPos.Store.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Store.Infrastructure.Persistence.Configurations;

public class FinancialShiftsConfiguration : IEntityTypeConfiguration<FinancialShifts>
{
    public void Configure(EntityTypeBuilder<FinancialShifts> builder)
    {
        builder.Property(fs => fs.Id);
        
        builder.Property(fs => fs.FinancialShiftConfigId)
            .IsRequired();
        builder.Property(fs => fs.OpeningTimestamp)
            .IsRequired();
        builder.Property(fs => fs.OpenedByAccountId)
            .IsRequired();
        builder.Property(fs => fs.OpeningCashExpected)
            .IsRequired();
        builder.Property(fs => fs.OpeningCashActual)
            .IsRequired();
        builder.Property(fs => fs.OpeningDifferenceReason)
            .HasMaxLength(1000);
        builder.Property(fs => fs.Status)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (EFinancialShiftStatus)Enum.Parse(typeof(EFinancialShiftStatus), v)
            );
        builder.HasOne(fs => fs.FinancialShiftConfigs)
            .WithMany(fsc => fsc.FinancialShifts)
            .HasForeignKey(fs => fs.FinancialShiftConfigId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}