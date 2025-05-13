using DimPos.Catalog.Domain.Entities.Common;
using DimPos.Catalog.Domain.Enums;

namespace DimPos.Catalog.Domain.Entities;

public class Products : EntityAuditBase<Guid>
{
    public string? Code { get; set; }
    public string? AlternativeCode { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsHasVariants { get; set; } = false;
    public bool? IsHasRecipe { get; set; } = false;
    public EProductStatus? Status { get; set; }
    public bool? IsAvailable { get; set; } = true;
    public int? DisplayOrder { get; set; }
    public bool? IsMenuDisplay { get; set; } = false;
    public int? SaleType { get; set; }
    public bool? IsMostOrdered { get; set; } = false;
    public string? Note { get; set; }
    public int? NumOfUserVoted { get; set; }
    
    public Guid? BrandId { get; set; }
    public Guid? CategoryId { get; set; }
    
    public virtual Categories? Category { get; set; }
    public virtual ICollection<ProductAttributes>? ProductAttributes { get; set; } = new List<ProductAttributes>();
    public virtual ICollection<ProductImages>? ProductImages { get; set; } = new List<ProductImages>();
    public virtual ICollection<ProductVariants>? ProductVariants { get; set; } = new List<ProductVariants>();
    public virtual IEnumerable<ProductModifierGroups>? ProductModifierGroups { get; set; } = new List<ProductModifierGroups>();
}