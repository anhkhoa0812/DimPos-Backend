using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class BasePrice : EntityAuditBase<Guid>
{
    public string CurrencyCode { get; set; }
    public decimal Price { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid BrandId { get; set; }
    
    public virtual ICollection<BrandPriceHistory>? BrandPriceHistories { get; set; } = new List<BrandPriceHistory>();
}