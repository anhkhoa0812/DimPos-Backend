using MassTransit;

namespace SharedProject.Events.UpdateInventoryForInternalOrder;

public class ChangeErrorStatusForStorePurchaseOrderRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid StoreId { get; set; } = default!;
    public Guid StorePurchaseOrderId { get; set; } = default!;
}