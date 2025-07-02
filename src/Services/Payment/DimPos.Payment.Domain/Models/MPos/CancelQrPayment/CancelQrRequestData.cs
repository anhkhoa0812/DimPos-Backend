using System.Text.Json.Serialization;

namespace DimPos.Payment.Domain.Models.MPos.CancelQrPayment;

public class CancelQrRequestData
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; } 
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    [JsonPropertyName("muid")]
    public string Muid { get; set; }
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
    [JsonPropertyName("qrType")]
    public string QrType { get; set; }
}