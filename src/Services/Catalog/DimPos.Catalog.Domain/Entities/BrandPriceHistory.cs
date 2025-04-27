using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class BrandPriceHistory : EntityBase<Guid>
{
    public string? CurrencyCode { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal? NewPrice { get; set; }
    public DateTime? ChangedAt { get; set; }
    public Guid? ChangedBy { get; set; }
    public Guid? ProductVariantId { get; set; }
    public Guid? BrandPriceId { get; set; }
    public virtual BasePrice? BrandPrice { get; set; }
}