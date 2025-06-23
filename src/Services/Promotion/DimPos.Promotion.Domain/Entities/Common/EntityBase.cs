using DimPos.Promotion.Domain.Entities.Common.Interface;

namespace DimPos.Promotion.Domain.Entities.Common;

public class EntityBase<TKey> : IEntityBase<TKey>
{
    public TKey Id { get; set; }
}