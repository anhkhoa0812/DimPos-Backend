using DimPos.MenuCombo.Domain.Entities.Common.Interface;

namespace DimPos.MenuCombo.Domain.Entities.Common;

public abstract class EntityAuditBase<T> : EntityBase<T>, IAuditable
{
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}