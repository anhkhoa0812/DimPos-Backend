using DimPos.Order.Domain.Enums;

namespace DimPos.Order.Domain.Models.Response;

public class GetOrderByStoreResponse
{
    public Guid Id { get; set; }
    public EOrderType Type { get; set; }
    public EOrderStatus Status { get; set; }
    public string? CustomerNameSnapshot { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? TableNumberDineIn { get; set; }
    public DateTime? PickupTime { get; set; }
    public bool IsNeedToUpdateInventory { get; set; }
    public List<GetOrderItemByStoreResponse> OrderItems { get; set; } = new();
}
public class GetOrderItemByStoreResponse
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductVariantNameSnapshot { get; set; } = String.Empty;
    public int Quantity { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public decimal TotalPriceBeforeItemDiscount { get; set; }
    public string? Note { get; set; }
    public List<GetOrderItemExtrasByStoreResponse>? OrderItemExtras { get; set; } = new List<GetOrderItemExtrasByStoreResponse>();
}
public class GetOrderItemExtrasByStoreResponse
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string ProductVariantNameSnapshot { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
}