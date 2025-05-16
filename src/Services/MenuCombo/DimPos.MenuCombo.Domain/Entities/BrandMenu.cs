using DimPos.MenuCombo.Domain.Entities.Common;
using DimPos.MenuCombo.Domain.Enums;

namespace DimPos.MenuCombo.Domain.Entities;

public class BrandMenu : EntityAuditBase<Guid>
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public EBrandMenuType? Type { get; set; }
    public bool IsActiveByBrand { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public Guid BrandId { get; set; }
    
    public virtual ICollection<BrandMenuItems> MenuItems { get; set; } = new List<BrandMenuItems>();
}