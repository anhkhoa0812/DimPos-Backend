using DimPos.MenuCombo.Domain.Entities.Common.Interface;

namespace DimPos.MenuCombo.Domain.Entities.Common;

public class EntityBase<TKey> : IEntityBase<TKey>
{
    public TKey Id { get; set; }
}