using DimPos.Promotion.Domain.Models.Common;
using Mediator;

namespace DimPos.Promotion.Application.Features.PromotionRule.Query.GetPromotionRuleById;

public class GetPromotionRuleByIdQuery : IRequest<ApiResponse>
{
    public Guid PromotionRuleId { get; set; }
}