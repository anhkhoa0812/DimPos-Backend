using System.Text.Json.Serialization;

namespace DimPos.Payment.Domain.Models.MPos.GetQrStatus;

public class GetQrStatusRequestData
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    [JsonPropertyName("muid")]
    public string Muid { get; set; }
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
}