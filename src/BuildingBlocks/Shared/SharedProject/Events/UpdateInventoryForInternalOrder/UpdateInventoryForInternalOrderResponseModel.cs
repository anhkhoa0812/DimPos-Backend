using MassTransit;

namespace SharedProject.Events.UpdateInventoryForInternalOrder;

public class UpdateInventoryForInternalOrderResponseModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid StoreId { get; set; } = default!;
    public Guid StorePurchaseOrderId { get; set; } = default!;
}