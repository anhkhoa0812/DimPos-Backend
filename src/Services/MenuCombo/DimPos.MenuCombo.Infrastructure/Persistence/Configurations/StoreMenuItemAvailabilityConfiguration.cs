using DimPos.MenuCombo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.MenuCombo.Infrastructure.Persistence.Configurations;

public class StoreMenuItemAvailabilityConfiguration : IEntityTypeConfiguration<StoreMenuItemAvailability>
{
    public void Configure(EntityTypeBuilder<StoreMenuItemAvailability> builder)
    {
        builder.HasKey(smia => smia.Id);
    }
}