using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;

public class ModifierGroups : EntityAuditBase<Guid>
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? SelectedType { get; set; }
    public int? DisplayOrder { get; set; }
    public int? Status { get; set; }
    public Guid? BrandId { get; set; }
    
    public virtual IEnumerable<ModifierOptions>? ModifierOptions { get; set; } = new List<ModifierOptions>();
    public virtual IEnumerable<ProductModifierGroups>? ProductModifierGroups { get; set; } = new List<ProductModifierGroups>();
}