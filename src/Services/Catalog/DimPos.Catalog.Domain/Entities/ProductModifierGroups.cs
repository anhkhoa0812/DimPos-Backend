using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class ProductModifierGroups : EntityAuditBase<Guid>
{
    public Guid ProductId { get; set; }
    public virtual Products Product { get; set; }
    public Guid ModifierGroupId { get; set; }
    public virtual ModifierGroups ModifierGroup { get; set; }
}