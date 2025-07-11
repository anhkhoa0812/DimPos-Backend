using MassTransit;

namespace SharedProject.Events.UpdateInventoryForInternalOrder;

public class UpdateInventoryForInternalOrderErrorModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid StorePurchaseOrderId { get; set; }
    public Guid StoreId { get; set; }
}