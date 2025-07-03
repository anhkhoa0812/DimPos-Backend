namespace DimPos.Payment.Domain.Models.Payment;

public class CreateCancelQrRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string CredentialsConfig { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}