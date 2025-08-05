using MassTransit;

namespace SharedProject.Events.Order.CancelOrder;

public class UpdateInventoryForCancelOrderErrorModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public Guid StoreId { get; set; }
    public Guid AccountId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}