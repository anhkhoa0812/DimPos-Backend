using System.Text.Json.Serialization;

namespace DimPos.Payment.Domain.Models.MPos.EDCPayment;

public class EDCPaymentRequestData
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    [JsonPropertyName("posId")]
    public string PosId { get; set; }
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
    [JsonPropertyName("Description")]
    public string? Description { get; set; }
    [JsonPropertyName("paymentType")]
    public string? PaymentType { get; set; }
    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; set; }
}