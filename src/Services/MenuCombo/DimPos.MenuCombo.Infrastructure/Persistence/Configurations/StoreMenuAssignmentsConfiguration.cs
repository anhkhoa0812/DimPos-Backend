using DimPos.MenuCombo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.MenuCombo.Infrastructure.Persistence.Configurations;

public class StoreMenuAssignmentsConfiguration : IEntityTypeConfiguration<StoreMenuAssignments>
{
    public void Configure(EntityTypeBuilder<StoreMenuAssignments> builder)
    {
        builder.HasKey(sma => sma.Id);
        builder.Property(sma => sma.StoreId)
            .IsRequired();
        builder.Property(sma => sma.BrandMenuId)
            .IsRequired();
        builder.Property(sma => sma.IsActiveAtStore)
            .IsRequired();
        builder.HasOne(sma => sma.BrandMenu)
            .WithMany(bm => bm.StoreMenuAssignments)
            .HasForeignKey(sma => sma.BrandMenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}