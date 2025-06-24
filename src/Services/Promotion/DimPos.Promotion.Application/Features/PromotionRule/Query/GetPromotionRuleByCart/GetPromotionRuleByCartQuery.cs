using DimPos.Promotion.Domain.Models.Common;
using Mediator;

namespace DimPos.Promotion.Application.Features.PromotionRule.Query.GetPromotionRuleByCart;

public class GetPromotionRuleByCartQuery : IRequest<ApiResponse>
{
    public Guid CartId { get; set; }
}