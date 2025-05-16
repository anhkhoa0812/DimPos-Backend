using DimPos.MenuCombo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.MenuCombo.Infrastructure.Persistence.Configurations;

public class StoreMenuItemAvailabilityConfiguration : IEntityTypeConfiguration<StoreMenuItemAvailability>
{
    public void Configure(EntityTypeBuilder<StoreMenuItemAvailability> builder)
    {
        builder.HasKey(smia => smia.Id);
        builder.Property(smia => smia.StoreMenuAssignmentId)
            .IsRequired();
        builder.Property(smia => smia.BrandMenuItemId)
            .IsRequired();
        builder.Property(smia => smia.IsActiveAtStore)
            .IsRequired();
    }
}