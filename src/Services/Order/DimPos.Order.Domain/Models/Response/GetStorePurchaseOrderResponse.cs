using DimPos.Order.Domain.Enums;

namespace DimPos.Order.Domain.Models.Response;

public class GetStorePurchaseOrderResponse
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public EStorePurchaseOrderStatus Status { get; set; }
    public string? CancellationRequestReasonByStore { get; set; }
    public string? CancellationReasonByBrand { get; set; }
    public string? NoteFromStore { get; set; }
    public string? NoteFromBrand { get; set; }
    public decimal EstimatedTotalValue { get; set; }
    public DateTime? ConfirmedByBrandAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid CreatedByAccountId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public List<GetStorePurchaseOrderItemByOrderResponse> StorePurchaseOrderItems { get; set; } = new List<GetStorePurchaseOrderItemByOrderResponse>();
}
public class GetStorePurchaseOrderItemByOrderResponse {
    public Guid Id { get; set; }
    public Guid ProductVariantIdSnapshot { get; set; }
    public string ProductVariantNameSnapshot { get; set; } = String.Empty;
    public decimal ProductVariantPriceSnapshot { get; set; }
    public decimal TotalPriceOfOrderItems { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal? ApprovedQuantityByBrand { get; set; }
}