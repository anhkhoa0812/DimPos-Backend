using DimPos.MenuCombo.Domain.Entities.Common;

namespace DimPos.MenuCombo.Domain.Entities;
 
public class BrandMenuItems : EntityAuditBase<Guid>
{
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid MenuId { get; set; }
    
    public virtual BrandMenu Menu { get; set; }
    public virtual ICollection<StoreMenuItemAvailability>? StoreMenuItemAvailability { get; set; } =
        new List<StoreMenuItemAvailability>();
}