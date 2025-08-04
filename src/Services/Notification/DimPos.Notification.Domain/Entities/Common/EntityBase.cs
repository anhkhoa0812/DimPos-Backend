using DimPos.Notification.Domain.Entities.Common.Interface;

namespace DimPos.Notification.Domain.Entities.Common;

public class EntityBase<TKey> : IEntityBase<TKey>
{
    public TKey Id { get; set; }
}