using DimPos.Identity.Domain.Entities.Common.Interface;

namespace DimPos.Identity.Domain.Entities.Common;

public class EntityBase<TKey> : IEntityBase<TKey>
{
    public TKey Id { get; set; }
}