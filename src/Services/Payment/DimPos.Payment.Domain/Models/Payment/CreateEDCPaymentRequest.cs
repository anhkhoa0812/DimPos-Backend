using DimPos.Payment.Domain.Enums;

namespace DimPos.Payment.Domain.Models.Payment;

public class CreateEDCPaymentRequest
{
    public Guid OrderId { get; set; }
    public EEDCPaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string CredentialsConfig { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}