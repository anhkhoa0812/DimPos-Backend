namespace DimPos.Basket.Application.Models.Request;

public class AddToCartRequest
{
    public Guid ProductVariantId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string ProductVariantNameSnapshot { get; set; } = string.Empty;
    public string? ProductImageUrlSnapshot { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceAtAdditionSnapshot { get; set; }
    public string? NotesForItem { get; set; }
    public List<CartModifierGroupItemRequest>? ModifierGroupItems { get; set; }
    public List<CartExtraItemRequest>? ExtraItems { get; set; }
}
public class CartModifierGroupItemRequest
{
    public Guid ModifierGroupId { get; set; }
    public Guid ModifierOptionId { get; set; }
    public string ModifierGroupNameSnapshot { get; set; } = string.Empty;
    public string ModifierOptionSnapshot { get; set; } = string.Empty;
    public Guid? RelatedComboProductVariantItemId { get; set; }
    public string? RelatedComboProductVariantItemName { get; set; }
}
public class CartExtraItemRequest
{
    public Guid ExtraProductVariantId { get; set; }
    public string ExtraProductVariantNameSnapshot { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPriceAtAdditionSnapshot { get; set; }
    public Guid? RelatedProductVariantId { get; set; }
}