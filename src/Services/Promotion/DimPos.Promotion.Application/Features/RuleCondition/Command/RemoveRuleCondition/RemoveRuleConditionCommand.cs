using DimPos.Promotion.Domain.Models.Common;
using Mediator;

namespace DimPos.Promotion.Application.Features.RuleCondition.Command.RemoveRuleCondition;

public class RemoveRuleConditionCommand : IRequest<ApiResponse>
{
    public Guid PromotionRuleId { get; set; }
    public Guid RuleConditionId { get; set; }
}