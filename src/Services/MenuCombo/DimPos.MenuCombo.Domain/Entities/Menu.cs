using DimPos.MenuCombo.Domain.Entities.Common;

namespace DimPos.MenuCombo.Domain.Entities;

public class Menu : EntityAuditBase<Guid>
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public bool? IsActiveByBrand { get; set; }
    public Guid? BrandId { get; set; }
    
    public virtual IEnumerable<MenuItems> MenuItems { get; set; } = new List<MenuItems>();
}