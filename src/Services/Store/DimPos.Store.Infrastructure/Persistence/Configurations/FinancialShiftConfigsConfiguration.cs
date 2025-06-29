using DimPos.Store.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Store.Infrastructure.Persistence.Configurations;

public class FinancialShiftConfigsConfiguration : IEntityTypeConfiguration<FinancialShiftConfigs>
{
    public void Configure(EntityTypeBuilder<FinancialShiftConfigs> builder)
    {
        builder.HasKey(fsc => fsc.Id);
        
        builder.Property(fsc => fsc.StoreId)
            .IsRequired();

        builder.Property(fsc => fsc.OpeningTime)
            .IsRequired();

        builder.Property(fsc => fsc.ClosingTime)
            .IsRequired();

        builder.Property(fsc => fsc.CreatedByAccountId)
            .IsRequired();
        
        builder.HasOne(fsc => fsc.Store)
            .WithMany(s => s.FinancialShiftConfigs)
            .HasForeignKey(fsc => fsc.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}