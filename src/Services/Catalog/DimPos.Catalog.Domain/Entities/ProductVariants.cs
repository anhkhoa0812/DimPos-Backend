using DimPos.Catalog.Domain.Entities.Common;
using DimPos.Catalog.Domain.Enums;

namespace DimPos.Catalog.Domain.Entities;

public class ProductVariants : EntityBase<Guid>
{
    public string Code { get; set; }
    public string? AlternativeCode { get; set; }
    public string Name { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal Price { get; set; }
    public decimal? PriceCOGS { get; set; }
    public bool IsActive { get; set; }
    public string? Size { get; set; }
    public bool? IsMenuDisplay { get; set; }
    public int? DisplayOrder { get; set; }
    public EProductVariantStatus Status { get; set; }
    public Guid ProductId { get; set; }
    public virtual Products Product { get; set; }
    // public virtual IEnumerable<Recipes>? Recipes { get; set; }
}