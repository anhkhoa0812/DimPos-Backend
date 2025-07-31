using MassTransit;

namespace SharedProject.Events.Payment.UpdatePaymentTransaction;

public class UpdateOrderStatusResponseModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public Guid PaymentTransactionId { get; set; }
}