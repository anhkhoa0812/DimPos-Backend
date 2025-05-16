using DimPos.Store.Domain.Entities.Common;
using DimPos.Store.Domain.Enums;

namespace DimPos.Store.Domain.Entities;

public class StoreAccounts: EntityAuditBase<Guid>
{
    public Guid StoreId { get; set; }
    public Guid AccountId { get; set; }
    public DateTime AssignAt { get; set; }
    public EStoreRole Role { get; set; }
    public virtual Store Store { get; set; }
}