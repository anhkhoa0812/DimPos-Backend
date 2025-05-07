using DimPos.Brand.Domain.Entities.Common.Interface;

namespace DimPos.Brand.Domain.Entities.Common;

public class EntityBase<TKey> : IEntityBase<TKey>
{
    public TKey Id { get; set; }
}