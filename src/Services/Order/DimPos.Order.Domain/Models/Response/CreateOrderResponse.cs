namespace DimPos.Order.Domain.Models.Response;

public class CreateOrderResponse 
{
    public Guid OrderId { get; set; }
    public string? PaymentUrl { get; set; }
}