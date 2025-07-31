using MassTransit;

namespace SharedProject.Events.Order.UpdateInventoryForSuccessOrder;

public class UpdateInventoryForSuccessOrderErrorModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public Guid StoreId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}