using DimPos.Promotion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Promotion.Infrastructure.Persistence.Configurations;

public class CampaignStoresConfiguration : IEntityTypeConfiguration<CampaignStores>
{
    public void Configure(EntityTypeBuilder<CampaignStores> builder)
    {
        builder.HasKey(cs => cs.Id);
        
        builder.Property(cs => cs.CampaignId)
            .IsRequired();
        builder.Property(cs => cs.StoreId)
            .IsRequired();
        builder.Property(cs => cs.IsActiveAtStore)
            .IsRequired();
        
        builder.HasOne(cs => cs.Campaign)
            .WithMany(c => c.CampaignStores)
            .HasForeignKey(cs => cs.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(x => new { x.CampaignId, x.StoreId })
            .IsUnique();
    }
}