using MassTransit;

namespace SharedProject.Events.UpdateInventoryForInternalOrder;

public class UpdateInventoryForInternalOrderErrorModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid AccountId { get; set; }
    public Guid StorePurchaseOrderId { get; set; }
    public Guid StoreId { get; set; }
    public string Message { get; set; } = string.Empty;
}