using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Promotion.Infrastructure.Persistence.Configurations;

public class CampaignsConfiguration : IEntityTypeConfiguration<Campaigns>
{
    public void Configure(EntityTypeBuilder<Campaigns> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.BrandId)
            .IsRequired();
        
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.StartDate)
            .IsRequired();

        builder.Property(c => c.EndDate)
            .IsRequired();
        
        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (ECampaignsStatus)Enum.Parse(typeof(ECampaignsStatus), v)
            );
        builder.Property(c => c.Channel)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (ECampaignChannel)Enum.Parse(typeof(ECampaignChannel), v)
            );
        builder.Property(c => c.Priority)
            .IsRequired();
        
    }
}