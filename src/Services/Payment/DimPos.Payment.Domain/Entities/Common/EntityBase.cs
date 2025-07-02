using DimPos.Payment.Domain.Entities.Common.Interface;

namespace DimPos.Payment.Domain.Entities.Common;

public class EntityBase<TKey> : IEntityBase<TKey>
{
    public TKey Id { get; set; }
}