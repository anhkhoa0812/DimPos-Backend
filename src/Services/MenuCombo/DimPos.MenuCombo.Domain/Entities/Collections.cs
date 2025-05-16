using DimPos.MenuCombo.Domain.Entities.Common;

namespace DimPos.MenuCombo.Domain.Entities;

public class Collections : EntityAuditBase<Guid>
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActiveByBrand { get; set; }
    public string? SKU { get; set; }
    public Guid BrandId { get; set; }
    
    public virtual IEnumerable<CollectionItems> CollectionItems { get; set; } = new List<CollectionItems>();
}