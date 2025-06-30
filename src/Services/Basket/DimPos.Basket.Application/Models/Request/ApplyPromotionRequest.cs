using DimPos.Basket.Application.Enums;

namespace DimPos.Basket.Application.Models.Request;

public class ApplyPromotionRequest
{
    public Guid PromotionRuleId {get; set; }
    public string PromotionNameSnapshot { get; set; } = string.Empty;
    public EActionType ActionType { get; set; }
    public string ActionValue { get; set; } = string.Empty;
    public List<ConditionRuleRequest> ConditionRules { get; set; } = new List<ConditionRuleRequest>();
    public List<Guid>? TargetCriteriaForItemAction { get; set; }
    public decimal MaxDiscountAmountForPercentage { get; set; }
    public List<Guid>? ApplicableCartItemIds { get; set; }
}

public class ConditionRuleRequest
{
    public EConditionType ConditionType { get; set; }
    public EOperator Operator { get; set; }
    public string ConditionValue { get; set; } = string.Empty;
}