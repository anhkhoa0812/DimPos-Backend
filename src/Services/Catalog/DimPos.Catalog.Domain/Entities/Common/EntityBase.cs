using DimPos.Catalog.Domain.Entities.Common.Interface;

namespace DimPos.Catalog.Domain.Entities.Common;

public class EntityBase<TKey> : IEntityBase<TKey>
{
    public TKey Id { get; set; }
}