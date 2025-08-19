using DimPos.Catalog.Domain.Entities.Common;
using DimPos.Catalog.Domain.Enums;

namespace DimPos.Catalog.Domain.Entities;

public class ProductVariants : EntityBase<Guid>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string? Size { get; set; }
    public int? DisplayOrder { get; set; }
    public Guid ProductId { get; set; }
    public virtual Products Product { get; set; }
    
    public virtual ICollection<RecipeItems>? RecipeItems { get; set; } = new List<RecipeItems>();
    public virtual ICollection<ProductComboItems>? ProductComboItems { get; set; } = new List<ProductComboItems>();
    public virtual ICollection<ProductExtraItems>? ProductExtraItems { get; set; } = new List<ProductExtraItems>();
}