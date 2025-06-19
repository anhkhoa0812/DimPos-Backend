using DimPos.Basket.Application.Enums;

namespace DimPos.Basket.Application.Models.Request;

public class UpdateCartRequest
{
    public Guid? CustomerId { get; set; }
    public EServiceMethod ServiceMethod { get; set; }
    public int? TakeNumberDineIn { get; set; }
    public DateTime? PickupTimeRequested { get; set; }
    public string? CustomerNotesForOrder { get; set; }
    public string? StaffNotesForOrder { get; set; }
    public int ItemCount { get; set; }
    public int TotalQuantityOfItems { get; set; }
}

public class UpdateCartItemRequest
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string ProductVariantNameSnapshot { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? NotesForItem { get; set; }
    public List<ModifierGroupItem>? ModifierGroupItems { get; set; }
}
public class UpdateCartModifierGroupItemRequest
{
    public Guid ModifierGroupId { get; set; }
    public Guid ModifierOptionId { get; set; }
    public string ModifierGroupNameSnapshot { get; set; } = string.Empty;
    public string ModifierOptionSnapshot { get; set; } = string.Empty;
}

public class AddToCartRequest
{
    public Guid ProductVariantId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string ProductVariantNameSnapshot { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPriceAtAdditionSnapshot { get; set; }
    public string? NotesForItem { get; set; }
    public List<UpdateCartModifierGroupItemRequest>? ModifierGroupItems { get; set; }
}