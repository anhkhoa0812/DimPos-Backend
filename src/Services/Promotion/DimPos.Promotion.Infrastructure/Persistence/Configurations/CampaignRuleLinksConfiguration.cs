using DimPos.Promotion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Promotion.Infrastructure.Persistence.Configurations;

public class CampaignRuleLinksConfiguration : IEntityTypeConfiguration<CampaignRuleLinks>
{
    public void Configure(EntityTypeBuilder<CampaignRuleLinks> builder)
    {
        builder.HasKey(crl => crl.Id);
        builder.Property(crl => crl.CampaignId)
            .IsRequired();
        builder.Property(crl => crl.PromotionRuleId)
            .IsRequired();
        //todo: Check Unique
        
        builder.HasOne(crl => crl.Campaign)
            .WithMany(c => c.CampaignRuleLinks)
            .HasForeignKey(crl => crl.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(crl => crl.PromotionRule)
            .WithMany(pr => pr.CampaignRuleLinks)
            .HasForeignKey(crl => crl.PromotionRuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}