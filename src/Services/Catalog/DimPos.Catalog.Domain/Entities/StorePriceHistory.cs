using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class StorePriceHistory : EntityBase<Guid>
{
    public string CurrencyCode { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime ChangedAt { get; set; }
    public Guid ChangedBy { get; set; }
    public Guid ProductVariantId {get; set;}
    public Guid StoreId { get; set; }
    public Guid StorePriceId { get; set; }
    public virtual StorePrice StorePrice { get; set; } = new StorePrice();
}