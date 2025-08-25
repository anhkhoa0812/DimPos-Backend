using DimPos.Catalog.Domain.Entities.Common;
using DimPos.Catalog.Domain.Enums;

namespace DimPos.Catalog.Domain.Entities;

public class Products : EntityAuditBase<Guid>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsHasVariants { get; set; }
    public int? DisplayOrder { get; set; }
    public string? Note { get; set; }
    public EProductType Type { get; set; }
    public bool IsCombo { get; set; }
    public bool IsExtra { get; set; }
    public Guid BrandId { get; set; }
    public Guid? CategoryId { get; set; }
    
    public virtual Categories? Category { get; set; }
    public virtual ICollection<ProductImages>? ProductImages { get; set; }
    public virtual ICollection<ProductVariants> ProductVariants { get; set; } = new List<ProductVariants>();
    public virtual ICollection<ProductModifierGroups>? ProductModifierGroups { get; set; } = new List<ProductModifierGroups>();
    public virtual ICollection<ProductComboItems>? ProductComboItems { get; set; } = new List<ProductComboItems>();
    public virtual ICollection<ProductExtraItems>? ProductExtraItems { get; set; } = new List<ProductExtraItems>();
}