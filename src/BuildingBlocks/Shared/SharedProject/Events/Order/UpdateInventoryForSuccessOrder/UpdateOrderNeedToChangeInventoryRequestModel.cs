using MassTransit;

namespace SharedProject.Events.Order.UpdateInventoryForSuccessOrder;

public class UpdateOrderNeedToChangeInventoryRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid StoreId { get; set; }
    public Guid OrderId { get; set; }
}