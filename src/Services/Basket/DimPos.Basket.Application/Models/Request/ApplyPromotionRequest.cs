using DimPos.Basket.Application.Enums;

namespace DimPos.Basket.Application.Models.Request;

public class ApplyPromotionRequest
{
    public Guid PromotionRuleId {get; set; }
    public string PromotionNameSnapshot { get; set; } = string.Empty;
    public EPromotionType PromotionTypeSnapshot { get; set; }
    public decimal DiscountValueCalculated { get; set; }
    public string? DescriptionOfBenefit { get; set; }
    public List<Guid>? ApplicableCartItemIds { get; set; }
}