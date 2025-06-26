using DimPos.Order.Domain.Entities.Common.Interface;

namespace DimPos.Order.Domain.Entities.Common;

public class EntityBase<TKey> : IEntityBase<TKey>
{
    public TKey Id { get; set; }
}