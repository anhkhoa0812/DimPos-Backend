namespace DimPos.Basket.Application.Models.Request;

public class UpdateCartItemRequest
{
    public string? NotesForItem { get; set; } 
    public int? Quantity { get; set; }
    public List<UpdateCartModifierGroupItemRequest>? ModifierGroupItems { get; set; }
    public List<UpdateCartExtraItemRequest>? ExtraItems { get; set; }
}
public class UpdateCartModifierGroupItemRequest
{
    public Guid ModifierGroupId { get; set; }
    public Guid ModifierOptionId { get; set; }
    public string ModifierGroupNameSnapshot { get; set; } = string.Empty;
    public string ModifierOptionSnapshot { get; set; } = string.Empty;
    public decimal PriceDeltaSnapshot { get; set; }
    public Guid? RelatedComboProductVariantItemId { get; set; }
    public string? RelatedComboProductVariantItemName { get; set; }
}
public class UpdateCartExtraItemRequest
{
    public Guid ExtraProductVariantId { get; set; }
    public string ExtraProductVariantNameSnapshot { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPriceAtAdditionSnapshot { get; set; }
    public Guid? RelatedProductVariantId { get; set; }
}