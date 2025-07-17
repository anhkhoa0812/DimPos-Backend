using DimPos.Promotion.Domain.Enums;
using DimPos.Promotion.Domain.Models.Common;
using Mediator;

namespace DimPos.Promotion.Application.Features.RuleCondition.Command.UpdateRuleCondition;

public class UpdateRuleConditionCommand : IRequest<ApiResponse>
{
    public Guid PromotionRuleId { get; set; }
    public Guid RuleConditionId { get; set; }
    public EOperator Operator { get; set; }
    public string Value { get; set; } = String.Empty;
}

public class UpdateRuleConditionRequest
{
    public EOperator Operator { get; set; }
    public string Value { get; set; } = String.Empty;
}