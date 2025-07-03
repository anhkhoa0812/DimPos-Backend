namespace DimPos.Payment.Domain.Models.Payment;

public class GetRefundEDCPaymentRequest
{
    public Guid OrderId { get; set; }
    public string CredentialsConfig { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}