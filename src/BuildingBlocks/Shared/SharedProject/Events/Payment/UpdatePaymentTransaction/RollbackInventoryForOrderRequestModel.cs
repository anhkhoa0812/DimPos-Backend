using MassTransit;

namespace SharedProject.Events.Payment.UpdatePaymentTransaction;

public class RollbackInventoryForOrderRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
}