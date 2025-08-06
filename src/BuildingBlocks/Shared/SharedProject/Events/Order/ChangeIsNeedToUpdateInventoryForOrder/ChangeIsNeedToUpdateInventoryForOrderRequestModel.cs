using MassTransit;

namespace SharedProject.Events.Order.ChangeIsNeedToUpdateInventoryForOrder;

public class ChangeIsNeedToUpdateInventoryForOrderRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
}