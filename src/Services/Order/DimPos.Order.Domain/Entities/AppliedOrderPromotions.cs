using DimPos.Order.Domain.Entities.Common;

namespace DimPos.Order.Domain.Entities;

public class AppliedOrderPromotions : EntityBase<Guid>
{
    public Guid OrderId { get; set; }
    public Guid PromotionRuleId { get; set; }
    public string PromotionNameSnapshot { get; set; } = string.Empty;
    public string PromotionTypeSnapshot { get; set; } = string.Empty;
    public string? PromotionDescriptionSnapshot { get; set; }
    public decimal DiscountAmountApplied { get; set; }
    public virtual Orders Order { get; set; } = new Orders();
}