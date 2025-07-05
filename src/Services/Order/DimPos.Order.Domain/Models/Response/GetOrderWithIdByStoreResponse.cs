using DimPos.Order.Domain.Enums;

namespace DimPos.Order.Domain.Models.Response;

public class GetOrderWithIdByStoreResponse
{
    public Guid Id { get; set; }
    public EOrderType Type { get; set; }
    public EOrderStatus Status { get; set; }
    public string? CustomerNameSnapshot { get; set; }
    public decimal SubTotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal CashRoundingAmount { get; set; }
    public DateTime? PickupTime { get; set; }
    public string? Note { get; set; }
    public string SystemPaymentMethodNameSnapshot { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public List<GetOrderItemsByOrderByIdResponse> OrderItems { get; set; } = new List<GetOrderItemsByOrderByIdResponse>();
    public List<GetAppliedOrderPromotionsByOrderIdResponse>? AppliedOrderPromotions { get; set; }
    
}
public class GetOrderItemsByOrderByIdResponse
{
    public Guid Id { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string ProductVariantNameSnapshot { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public decimal TotalPriceBeforeItemDiscount { get; set; }
    public string? Note { get; set; }
}
public class GetAppliedOrderPromotionsByOrderIdResponse
{
    public Guid Id { get; set; }
    public string PromotionNameSnapshot { get; set; } = string.Empty;
    public string PromotionTypeSnapshot { get; set; } = string.Empty; 
    public string? PromotionDescriptionSnapshot { get; set; }
    public decimal DiscountAmountApplied { get; set; }
}