using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Promotion.Infrastructure.Persistence.Configurations;

public class RuleActionsConfiguration : IEntityTypeConfiguration<RuleActions>
{
    public void Configure(EntityTypeBuilder<RuleActions> builder)
    {
        builder.HasKey(ra => ra.Id);
        
        builder.Property(ra => ra.PromotionRuleId)
            .IsRequired();
        
        builder.Property(ra => ra.ActionType)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (EActionType)Enum.Parse(typeof(EActionType), v)
            );

        builder.Property(ra => ra.Value)
            .IsRequired();
        
        builder.HasOne(ra => ra.PromotionRule)
            .WithOne(ra => ra.RuleActions)
            .HasForeignKey<RuleActions>(ra => ra.PromotionRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(ra => ra.TargetCriteriaForItemAction)
            .IsRequired();

        builder.Property(ra => ra.MaxDiscountAmountForPercentage)
            .IsRequired();
    }
}