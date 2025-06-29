using DimPos.Promotion.Domain.Enums;

namespace DimPos.Promotion.Domain.Models.PromotionRules;

public class PromotionRulesByCartResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public bool IsValid { get; set; }
    public List<RuleConditionsForCartResponse> RuleConditions { get; set; }
    public RuleActionsForCartResponse RuleAction { get; set; }
}
public class RuleActionsForCartResponse
{
    public Guid Id { get; set; }
    public EActionType ActionType { get; set; }
    public string Value { get; set; } = string.Empty;
    public List<Guid>? TargetCriteriaForItemAction { get; set; }
    public decimal? MaxDiscountAmountForPercentage { get; set; }
}
public class RuleConditionsForCartResponse
{
    public Guid Id { get; set; }
    public EConditionType ConditionType { get; set; }
    public EOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
}