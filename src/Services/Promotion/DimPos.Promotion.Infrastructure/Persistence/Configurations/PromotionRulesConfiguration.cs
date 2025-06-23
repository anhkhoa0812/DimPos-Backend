using DimPos.Promotion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Promotion.Infrastructure.Persistence.Configurations;

public class PromotionRulesConfiguration : IEntityTypeConfiguration<PromotionRules>
{
    public void Configure(EntityTypeBuilder<PromotionRules> builder)
    {
        builder.HasKey(cr => cr.Id);
        builder.Property(cr => cr.BrandId)
            .IsRequired();
        builder.Property(cr => cr.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(cr => cr.Description)
            .IsRequired()
            .HasMaxLength(1000);
        builder.Property(cr => cr.IsActive)
            .IsRequired();
        builder.Property(cr => cr.Priority)
            .IsRequired();

    }
}