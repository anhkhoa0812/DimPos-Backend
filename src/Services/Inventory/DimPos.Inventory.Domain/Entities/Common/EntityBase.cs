using DimPos.Inventory.Domain.Entities.Common.Interface;

namespace DimPos.Inventory.Domain.Entities.Common;

public class EntityBase<TKey> : IEntityBase<TKey>
{
    public TKey Id { get; set; }
}