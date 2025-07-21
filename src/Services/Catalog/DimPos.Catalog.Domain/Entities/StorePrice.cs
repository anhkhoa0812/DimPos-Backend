using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class StorePrice : EntityAuditBase<Guid>
{
    public string CurrencyCode { get; set; }
    public decimal OverridePrice { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public bool IsActiveAtStore { get; set; }
    public Guid StoreId { get; set; }
    public Guid ProductVariantId { get; set; }
    
    public virtual ICollection<StorePriceHistory>? StorePriceHistories { get; set; } = new List<StorePriceHistory>();
}