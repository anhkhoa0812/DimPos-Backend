using DimPos.Order.Domain.Entities.Common;

namespace DimPos.Order.Domain.Entities;

public class OrderItemExtras : EntityBase<Guid>
{
    public Guid OrderItemId { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string ProductVariantNameSnapshot { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    
    public virtual OrderItems OrderItem { get; set; } = null!;
}