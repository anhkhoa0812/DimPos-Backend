using DimPos.Order.Domain.Entities.Common;
using DimPos.Order.Domain.Enums;

namespace DimPos.Order.Domain.Entities;

public class StorePurchaseOrders : EntityAuditBase<Guid>
{
    public Guid StoreId { get; set; }
    public Guid BrandId { get; set; }
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
    
    public virtual ICollection<StorePurchaseOrderItems> StorePurchaseOrderItems { get; set; } = new List<StorePurchaseOrderItems>();
}