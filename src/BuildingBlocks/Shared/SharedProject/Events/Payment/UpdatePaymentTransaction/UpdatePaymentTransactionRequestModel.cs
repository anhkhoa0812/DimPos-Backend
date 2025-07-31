using MassTransit;

namespace SharedProject.Events.Payment.UpdatePaymentTransaction;

public class UpdatePaymentTransactionRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public MPosTransStatus TransStatus { get; set; }
    public string TransCode { get; set; }
    public long TransAmount { get; set; }
    // public string IssuerCode { get; set; }
    // public string Muid { get; set; }
    public Guid OrderId { get; set; }
    // public string PosId { get; set; }
}