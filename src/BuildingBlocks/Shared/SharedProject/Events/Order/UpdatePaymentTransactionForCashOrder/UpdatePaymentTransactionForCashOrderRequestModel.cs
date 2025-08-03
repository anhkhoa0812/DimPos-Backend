using MassTransit;

namespace SharedProject.Events.Order.UpdatePaymentTransactionForCashOrder;

public class UpdatePaymentTransactionForCashOrderRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public Guid StoreId { get; set; }
    public Guid PaymentTransactionId { get; set; }
}