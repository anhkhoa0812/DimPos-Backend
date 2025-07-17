using DimPos.Promotion.Domain.Enums;
using DimPos.Promotion.Domain.Models.Common;
using Mediator;

namespace DimPos.Promotion.Application.Features.RuleCondition.Command.AddRuleCondition;

public class AddRuleConditionCommand : IRequest<ApiResponse>
{
    public Guid PromotionRuleId { get; set; }
    public EConditionType ConditionType { get; set; }
    public EOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
}
public class AddRuleConditionRequest
{
    public EConditionType ConditionType { get; set; }
    public EOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
}