namespace DimPos.Payment.Domain.Models.Payment;

public class CreateCancelQrRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
}