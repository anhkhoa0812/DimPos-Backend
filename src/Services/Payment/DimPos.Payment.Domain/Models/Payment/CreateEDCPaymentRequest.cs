using DimPos.Payment.Domain.Enums;

namespace DimPos.Payment.Domain.Models.Payment;

public class CreateEDCPaymentRequest
{
    public EEDCPaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}