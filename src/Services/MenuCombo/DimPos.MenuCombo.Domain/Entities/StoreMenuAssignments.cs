using DimPos.MenuCombo.Domain.Entities.Common;

namespace DimPos.MenuCombo.Domain.Entities;

public class StoreMenuAssignments : EntityAuditBase<Guid>
{
    public bool IsActiveAtStore { get; set; }
    public DateTime? EffectiveAt { get; set; }
    public DateTime? EffectiveEnd { get; set; }
    public Guid StoreId { get; set; }
    public Guid BrandMenuId { get; set; }

    public virtual ICollection<StoreMenuItemAvailability>? StoreMenuItemAvailability { get; set; } =
        new List<StoreMenuItemAvailability>();

}