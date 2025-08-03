using MassTransit;

namespace SharedProject.Events.Order.UpdatePaymentTransactionForCashOrder;

public class UpdatePaymentTransactionForCashOrderResponseModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; } 
}