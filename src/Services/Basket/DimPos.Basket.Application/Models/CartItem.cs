namespace DimPos.Basket.Application.Models;

public class CartItem
{
     public Guid Id { get; set; }
     public Guid CartId { get; set; }
     public Guid ProductVariantId { get; set; }
     public string ProductNameSnapshot { get; set; } = string.Empty;
     public string ProductVariantNameSnapshot { get; set; } = string.Empty;

     public int Quantity { get; set; }
     public decimal UnitPriceAtAdditionSnapshot { get; set; }
     public decimal ItemSubtotalAmount { get; set; }
     public decimal ItemSpecificDiscountAmount { get; set; }
     public decimal ItemFinalPrice { get; set; }
     public string? NotesForItem { get; set; }
     public DateTime AddedAt { get; set; }
     public List<ModifierGroupItem>? ModifierGroupItems { get; set; }
}

public class ModifierGroupItem
{
     public Guid ModifierGroupId { get; set; }
     public Guid ModifierOptionId { get; set; }
     public string ModifierGroupNameSnapshot { get; set; } = string.Empty;
     public string ModifierOptionSnapshot { get; set; } = string.Empty;
}