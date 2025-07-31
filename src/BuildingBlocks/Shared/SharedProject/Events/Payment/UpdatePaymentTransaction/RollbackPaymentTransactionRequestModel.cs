using MassTransit;

namespace SharedProject.Events.Payment.UpdatePaymentTransaction;

public class RollbackPaymentTransactionRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public Guid PaymentTransactionId { get; set; }
    public bool IsPaymentSuccess { get; set; }
}