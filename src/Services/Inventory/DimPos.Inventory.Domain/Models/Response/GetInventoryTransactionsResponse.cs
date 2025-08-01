using DimPos.Inventory.Domain.Enums;

namespace DimPos.Inventory.Domain.Models.Response;

public class GetInventoryTransactionsResponse
{
    public Guid Id { get; set; }
    public EInventoryTransactionType Type { get; set; }
    public string? ReasonManualAdjustment { get; set; }
    public decimal QuantityChange { get; set; }
    public Guid? RelatedOrderId { get; set; }
    public Guid? RelatedStorePurchaseOrderItemId { get; set; }
    public Guid? AccountId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}