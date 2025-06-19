using DimPos.Basket.Application.Enums;

namespace DimPos.Basket.Application.Models;

public class CartAppliedPromotionDetail
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid PromotionRuleId { get; set; }
    public string PromotionNameSnapshot { get; set; } = string.Empty;
    public EPromotionType PromotionTypeSnapshot { get; set; }
    public decimal DiscountValueCalculated { get; set; }
    public string? DescriptionOfBenefit { get; set; }
    public List<Guid>? ApplicableCartItemIds { get; set; }
    public EConditionType ConditionType { get; set; }
    public EOperator Operator { get; set; }
    public string ConditionValue { get; set; } = string.Empty;
    public EActionType ActionType { get; set; }
    public string ActionValue { get; set; } = string.Empty;
    public List<Guid>? TargetCriteriaForItemAction { get; set; }
    public decimal? MaxDiscountAmountForPercentage { get; set; }
}