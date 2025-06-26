using DimPos.Order.Domain.Entities.Common;

namespace DimPos.Order.Domain.Entities;

public class AppliedOrderPromotions : EntityAuditBase<Guid>
{
    public Guid OrderId { get; set; }
    public Guid PromotionRuleId { get; set; }
    public string PromotionNameSnapshot { get; set; } = string.Empty;
    public string PromotionTypeSnapshot { get; set; } = string.Empty;
    public decimal DiscountAmountApplied { get; set; }
    public string? Description { get; set; }
    
    public virtual Orders Order { get; set; } = new Orders();
}