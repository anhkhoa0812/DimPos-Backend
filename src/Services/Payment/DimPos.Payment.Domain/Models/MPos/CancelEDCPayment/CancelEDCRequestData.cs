using System.Text.Json.Serialization;

namespace DimPos.Payment.Domain.Models.MPos.CancelEDCPayment;

public class CancelEDCRequestData
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    [JsonPropertyName("posId")]
    public string PosId { get; set; }
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
}