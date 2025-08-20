namespace DimPos.Basket.Application.Models;

public class CartItem
{
     public Guid Id { get; set; }
     public Guid CartId { get; set; }
     public Guid ProductVariantId { get; set; }
     public string ProductNameSnapshot { get; set; } = string.Empty;
     public string ProductVariantNameSnapshot { get; set; } = string.Empty;
     public string? ProductImageUrlSnapshot { get; set; } = string.Empty;
     public int Quantity { get; set; }
     public decimal UnitPriceAtAdditionSnapshot { get; set; }
     public decimal ItemSubtotalAmount { get; set; }
     public decimal TotalPriceOfProductExtraItems { get; set; }
     public string? NotesForItem { get; set; }
     public DateTime AddedAt { get; set; }
     public List<ModifierGroupItem>? ModifierGroupItems { get; set; }
     public List<ProductExtraItem>? ExtraItems { get; set; }
}

public class ModifierGroupItem
{
     public Guid ModifierGroupId { get; set; }
     public Guid ModifierOptionId { get; set; }
     public string ModifierGroupNameSnapshot { get; set; } = string.Empty;
     public string ModifierOptionSnapshot { get; set; } = string.Empty;
     public decimal PriceDeltaSnapshot { get; set; }
     public Guid? RelatedComboProductVariantItemId { get; set; }
     public string? RelatedComboProductVariantItemName { get; set; }
}

public class ProductExtraItem
{ 
     public Guid ExtraProductVariantId { get; set; }
     public string ExtraProductVariantNameSnapshot { get; set; } = string.Empty;
     public int Quantity { get; set; }
     public decimal UnitPriceAtAdditionSnapshot { get; set; }
     public Guid? RelatedProductVariantId { get; set; }
}