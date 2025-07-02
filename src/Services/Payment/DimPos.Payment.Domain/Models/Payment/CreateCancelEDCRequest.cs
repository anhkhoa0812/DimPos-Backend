namespace DimPos.Payment.Domain.Models.Payment;

public class CreateCancelEDCRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
}