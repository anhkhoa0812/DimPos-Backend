using DimPos.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Order.Infrastructure.Persistence.Configurations;

public class AppliedOrderPromotionsConfiguration : IEntityTypeConfiguration<AppliedOrderPromotions>
{
    public void Configure(EntityTypeBuilder<AppliedOrderPromotions> builder)
    {
        builder.Property(aop => aop.OrderId);
        builder.Property(aop => aop.PromotionRuleId);
        builder.Property(aop => aop.PromotionNameSnapshot)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(aop => aop.DiscountAmountApplied)
            .IsRequired();
        builder.Property(aop => aop.PromotionDescriptionSnapshot)
            .HasMaxLength(1000);
        
        builder.HasOne(aop => aop.Order)
            .WithMany(o => o.AppliedOrderPromotions)
            .HasForeignKey(aop => aop.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}