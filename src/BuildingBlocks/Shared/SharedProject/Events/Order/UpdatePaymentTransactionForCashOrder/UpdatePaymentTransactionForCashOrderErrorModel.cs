using MassTransit;

namespace SharedProject.Events.Order.UpdatePaymentTransactionForCashOrder;

public class UpdatePaymentTransactionForCashOrderErrorModel :  CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}