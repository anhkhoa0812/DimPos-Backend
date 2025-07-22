using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class ProductComboItems : EntityAuditBase<Guid>
{
    public int Quantity { get; set; }
    public int? DisplayOrder { get; set; }
    public Guid ProductId { get; set; }
    public Guid ItemProductVariantId { get; set; }
    
    public virtual Products Product { get; set; } = null!;
    public virtual ProductVariants ItemProductVariant { get; set; } = null!;
}