using System.Text.Json.Serialization;

namespace DimPos.Payment.Domain.Models.MPos.GetEDCStatus;

public class GetEDCStatusRequestData
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    [JsonPropertyName("posId")]
    public string PosId { get; set; }
}