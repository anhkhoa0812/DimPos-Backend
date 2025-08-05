using MassTransit;

namespace SharedProject.Events.Order.CancelOrder;

public class UpdateInventoryForCancelOrderResponseModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
}