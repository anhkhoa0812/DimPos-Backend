namespace DimPos.Payment.Domain.Models.Payment;

public record CreateQrPaymentRequest
{
    public double Amount { get; set; }
    public string? Description { get; set; }
}