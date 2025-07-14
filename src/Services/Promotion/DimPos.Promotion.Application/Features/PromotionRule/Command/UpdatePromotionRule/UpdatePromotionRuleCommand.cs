using DimPos.Promotion.Domain.Models.Common;
using Mediator;

namespace DimPos.Promotion.Application.Features.PromotionRule.Command.UpdatePromotionRule;

public class UpdatePromotionRuleCommand : IRequest<ApiResponse>
{
    public Guid PromotionRuleId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public bool? IsActive { get; set; }
    public int? Priority { get; set; }
}
public class UpdatePromotionRuleRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public bool? IsActive { get; set; }
    public int? Priority { get; set; }
}