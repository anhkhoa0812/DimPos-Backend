using System.Text.Json.Serialization;

namespace DimPos.Payment.Domain.Models.MPos.CancelQrPayment;

public class CancelQrResponseData
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; } 
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    [JsonPropertyName("merchantId")]
    public long MerchantId { get; set; }
    [JsonPropertyName("amount")]
    public long Amount { get; set; }
    [JsonPropertyName("muid")]
    public string Muid { get; set; }
}