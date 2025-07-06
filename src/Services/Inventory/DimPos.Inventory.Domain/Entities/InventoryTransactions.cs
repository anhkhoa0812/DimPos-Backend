using DimPos.Inventory.Domain.Entities.Common;
using DimPos.Inventory.Domain.Enums;

namespace DimPos.Inventory.Domain.Entities;

public class InventoryTransactions : EntityAuditBase<Guid>
{
    public Guid InventoryStockId { get; set; }
    public EInventoryTransactionType Type { get; set; }
    public string? ReasonManualAdjustment { get; set; }
    public decimal QuantityChange { get; set; }
    public Guid? RelatedOrderId { get; set; }
    public Guid? RelatedStorePurchaseOrderItemId { get; set; }
    public Guid? AccountId { get; set; }
    public string? Note { get; set; }
    
    public InventoryStock InventoryStock { get; set; }
}