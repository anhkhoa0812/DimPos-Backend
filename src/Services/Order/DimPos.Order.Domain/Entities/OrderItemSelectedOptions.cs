using DimPos.Order.Domain.Entities.Common;

namespace DimPos.Order.Domain.Entities;

public class OrderItemSelectedOptions : EntityBase<Guid>
{
    public Guid OrderItemId { get; set; }
    public Guid ModifierGroupId { get; set; }
    public Guid ModifierOptionId { get; set; }
    public string ModifierGroupSnapshot { get; set; } = string.Empty;
    public string ModifierOptionSnapshot { get; set; } = string.Empty;
    public Guid? RelatedComboProductVariantItemId { get; set; }
    public string? RelatedComboProductVariantItemName { get; set; }
    public virtual OrderItems OrderItem { get; set; } = null!;
}