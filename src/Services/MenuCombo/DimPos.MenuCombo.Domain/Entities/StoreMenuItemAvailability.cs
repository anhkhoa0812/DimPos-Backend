using DimPos.MenuCombo.Domain.Entities.Common;

namespace DimPos.MenuCombo.Domain.Entities;

public class StoreMenuItemAvailability : EntityAuditBase<Guid>
{
    public bool IsActiveAtStore { get; set; }
    public Guid BrandMenuItemId { get; set; }
    public Guid StoreMenuAssignmentId { get; set; }

    public virtual StoreMenuAssignments StoreMenuAssignment { get; set; } = null!;
    public virtual BrandMenuItems BrandMenuItem { get; set; } = null!;
}