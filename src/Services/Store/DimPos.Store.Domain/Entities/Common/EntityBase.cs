using DimPos.Store.Domain.Entities.Common.Interface;

namespace DimPos.Store.Domain.Entities.Common;

public class EntityBase<TKey> : IEntityBase<TKey>
{
    public TKey Id { get; set; }
}