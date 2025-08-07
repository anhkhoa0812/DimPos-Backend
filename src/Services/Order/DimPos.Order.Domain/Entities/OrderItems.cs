using DimPos.Order.Domain.Entities.Common;

namespace DimPos.Order.Domain.Entities;

public class OrderItems : EntityAuditBase<Guid>
{
    public Guid OrderId { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string ProductVariantNameSnapshot { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public decimal TotalPriceBeforeItemDiscount { get; set; }
    public string? Note { get; set; }
    
    public virtual Orders Order { get; set; }
    public virtual ICollection<OrderItemSelectedOptions>? OrderItemSelectedOptions { get; set; } = new List<OrderItemSelectedOptions>();
}