using DimPos.Promotion.Domain.Entities;
using DimPos.Promotion.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Promotion.Infrastructure.Persistence.Configurations;

public class RuleConditionsConfiguration : IEntityTypeConfiguration<RuleConditions>
{
    public void Configure(EntityTypeBuilder<RuleConditions> builder)
    {
        builder.HasKey(rc => rc.Id);
        
        builder.Property(rc => rc.ConditionType)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (EConditionType)Enum.Parse(typeof(EConditionType), v)
            );
        
        builder.Property(rc => rc.Operator)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (EOperator)Enum.Parse(typeof(EOperator), v)
            );

        builder.Property(rc => rc.Value)
            .IsRequired();

        builder.Property(rc => rc.PromotionRuleId)
            .IsRequired();
        
        builder.HasOne(rc => rc.PromotionRule)
            .WithMany(pr => pr.RuleConditions)
            .HasForeignKey(rc => rc.PromotionRuleId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}