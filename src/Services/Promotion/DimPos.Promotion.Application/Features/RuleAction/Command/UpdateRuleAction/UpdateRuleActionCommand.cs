using DimPos.Promotion.Domain.Enums;
using DimPos.Promotion.Domain.Models.Common;
using Mediator;

namespace DimPos.Promotion.Application.Features.RuleAction.Command.UpdateRuleAction;

public class UpdateRuleActionCommand : IRequest<ApiResponse>
{
    public Guid PromotionRuleId { get; set; }
    public Guid RuleActionId { get; set; }
    public string Value { get; set; } = string.Empty;
    public EActionType ActionType { get; set; }
    public List<Guid>? TargetCriteriaForItemAction { get; set; }
    public decimal? MaxDiscountAmountForPercentage { get; set; }
}
public class UpdateRuleActionRequest
{
    public string Value { get; set; } = string.Empty;
    public EActionType ActionType { get; set; }

    public List<Guid>? TargetCriteriaForItemAction { get; set; }
    public decimal? MaxDiscountAmountForPercentage { get; set; }
}