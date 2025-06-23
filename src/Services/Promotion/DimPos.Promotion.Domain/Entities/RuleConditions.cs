using DimPos.Promotion.Domain.Entities.Common;
using DimPos.Promotion.Domain.Enums;

namespace DimPos.Promotion.Domain.Entities;

public class RuleConditions : EntityBase<Guid>
{
    public Guid PromotionRuleId { get; set; }
    public EConditionType ConditionType { get; set; }
    public EOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
    
    public virtual PromotionRules PromotionRule { get; set; } = new PromotionRules();
}