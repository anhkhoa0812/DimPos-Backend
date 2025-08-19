using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class ProductExtraItems : EntityAuditBase<Guid>
{
    public Guid ProductId { get; set; }
    public Guid ExtraProductVariantId { get; set; }
    
    public virtual Products Product { get; set; } = null!;
    public virtual ProductVariants ExtraProductVariant { get; set; } = null!;
}