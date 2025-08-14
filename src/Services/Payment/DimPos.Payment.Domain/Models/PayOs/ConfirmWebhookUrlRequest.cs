namespace DimPos.Payment.Domain.Models.PayOs;

public class ConfirmWebhookUrlRequest
{
    public string WebhookUrl { get; set; } = String.Empty;
    public Guid StoreId { get; set; }
    public string ConfigCredential { get; set; } = String.Empty;
}