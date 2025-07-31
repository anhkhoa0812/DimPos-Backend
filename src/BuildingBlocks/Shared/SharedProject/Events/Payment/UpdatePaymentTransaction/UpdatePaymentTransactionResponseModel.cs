using MassTransit;

namespace SharedProject.Events.Payment.UpdatePaymentTransaction;

public class UpdatePaymentTransactionResponseModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public MPosTransStatus TransStatus { get; set; }
    public Guid OrderId { get; set; }
    public Guid PaymentTransactionId { get; set; }
}