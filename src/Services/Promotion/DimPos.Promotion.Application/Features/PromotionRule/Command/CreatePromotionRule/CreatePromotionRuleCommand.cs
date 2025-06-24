using DimPos.Promotion.Domain.Enums;
using DimPos.Promotion.Domain.Models.Common;
using Mediator;

namespace DimPos.Promotion.Application.Features.PromotionRule.Command.CreatePromotionRule;

public class CreatePromotionRuleCommand : IRequest<ApiResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public List<CreateRuleConditionRequest> RuleConditions { get; set; } = new List<CreateRuleConditionRequest>();
    public CreateRuleActionRequest RuleActions { get; set; } = new CreateRuleActionRequest();
}

public class CreateRuleConditionRequest
{
    public EConditionType ConditionType { get; set; }
    public EOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
}
public class CreateRuleActionRequest
{
    public EActionType ActionType { get; set; }
    public string Value { get; set; } = string.Empty;
    public string TargetCriteriaForItemAction { get; set; } = string.Empty;
    public decimal MaxDiscountAmountForPercentage { get; set; }
}