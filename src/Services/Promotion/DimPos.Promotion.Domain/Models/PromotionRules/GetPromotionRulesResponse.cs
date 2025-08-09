using DimPos.Promotion.Domain.Enums;

namespace DimPos.Promotion.Domain.Models.PromotionRules;

public class GetPromotionRulesResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; 
    public string ShortDescription { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public RuleActionsResponse RuleActions { get; set; } = new ();
    public List<RuleConditionsResponse> RuleConditions { get; set; } = new();
}
public class RuleActionsResponse
{
    public Guid Id { get; set; }
    public EActionType ActionType { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? TargetCriteriaForItemAction { get; set; } = string.Empty;
    public decimal? MaxDiscountAmountForPercentage { get; set; }
}
public class RuleConditionsResponse
{
    public Guid Id { get; set; }
    public EConditionType ConditionType { get; set; }
    public EOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
}