using DimPos.Promotion.Domain.Models.Common;
using Mediator;

namespace DimPos.Promotion.Application.Features.PromotionRule.Command.AssignPromotionRules;

public class AssignPromotionRulesCommand : IRequest<ApiResponse>
{
    public Guid CampaignId { get; set; }
    public List<Guid>? PromotionRuleIds { get; set; }
}
public class AssignPromotionRulesRequest
{
    public List<Guid>? PromotionRuleIds { get; set; }
}