using DimPos.Catalog.Domain.Entities.Common;
using DimPos.Catalog.Domain.Enums;

namespace DimPos.Catalog.Domain.Entities;

public class ModifierGroups : EntityAuditBase<Guid>
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public ESelectedTypeModifier SelectedType { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public Guid BrandId { get; set; }
    
    public virtual ICollection<ModifierOptions>? ModifierOptions { get; set; } = new List<ModifierOptions>();
    public virtual ICollection<ProductModifierGroups>? ProductModifierGroups { get; set; } = new List<ProductModifierGroups>();
}