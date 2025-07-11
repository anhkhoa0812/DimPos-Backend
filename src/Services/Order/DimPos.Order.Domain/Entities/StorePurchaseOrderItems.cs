using DimPos.Order.Domain.Entities.Common;

namespace DimPos.Order.Domain.Entities;

public class StorePurchaseOrderItems : EntityBase<Guid>
{
    public Guid StorePurchaseOrderId { get; set; }
    public Guid ProductVariantIdSnapshot { get; set; }
    public string ProductVariantNameSnapshot { get; set; }
    public decimal ProductVariantPriceSnapshot { get; set; }
    public decimal TotalPriceOfOrderItems { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal? ApprovedQuantityByBrand { get; set; }
    public virtual StorePurchaseOrders StorePurchaseOrder { get; set; } = null!;
}