namespace DimPos.Basket.Application.Models.Request;

public class UpdateCartItemRequest
{
    public int? Quantity { get; set; }
    public List<UpdateCartModifierGroupItemRequest>? ModifierGroupItems { get; set; }
}
public class UpdateCartModifierGroupItemRequest
{
    public Guid ModifierGroupId { get; set; }
    public Guid ModifierOptionId { get; set; }
    public string ModifierGroupNameSnapshot { get; set; } = string.Empty;
    public string ModifierOptionSnapshot { get; set; } = string.Empty;
}