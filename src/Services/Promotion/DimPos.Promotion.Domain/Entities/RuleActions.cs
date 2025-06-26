using DimPos.Promotion.Domain.Entities.Common;
using DimPos.Promotion.Domain.Enums;

namespace DimPos.Promotion.Domain.Entities;

public class RuleActions : EntityBase<Guid>
{
    public Guid PromotionRuleId { get; set; }
    public EActionType ActionType { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? TargetCriteriaForItemAction { get; set; } = string.Empty;
    public decimal? MaxDiscountAmountForPercentage { get; set; }
    
    public virtual PromotionRules PromotionRule { get; set; }
}