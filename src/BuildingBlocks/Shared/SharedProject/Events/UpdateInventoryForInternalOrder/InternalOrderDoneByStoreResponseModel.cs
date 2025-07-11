using MassTransit;

namespace SharedProject.Events.UpdateInventoryForInternalOrder;

public class InternalOrderDoneByStoreResponseModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid StorePurchaseOrderId { get; set; }
    public Guid StoreId { get; set; }
    public List<StorePurchaseOrderItemRequestModel> StorePurchaseOrderItems { get; set; } = new List<StorePurchaseOrderItemRequestModel>();
}