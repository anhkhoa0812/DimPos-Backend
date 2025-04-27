using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class ProductVariants : EntityBase<Guid>
{
    public decimal? Price { get; set; }
    public int? Status { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountPrice { get; set; }
    public bool? IsMenuDisplay { get; set; }
    public bool? IsProductPriceBasedOnVariant { get; set; }
    
    public Guid? ProductId { get; set; }
    public virtual Products? Product { get; set; }
    
    public Guid? VariantOptionId { get; set; }
    public virtual VariantOptions? VariantOption { get; set; }
    
    public virtual IEnumerable<Recipes>? Recipes { get; set; }
}