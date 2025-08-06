using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class ModifierOptions : EntityAuditBase<Guid>
{
    public string Name { get; set; } = String.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public decimal? PriceDelta { get; set; }
    public Guid ModifierGroupId { get; set; }
    public virtual ModifierGroups ModifierGroup {get; set; }
}
