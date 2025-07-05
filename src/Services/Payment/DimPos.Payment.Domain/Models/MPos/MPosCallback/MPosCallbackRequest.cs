using System.Text.Json.Serialization;

namespace DimPos.Payment.Domain.Models.MPos.MPosCallback;

public class MPosCallbackRequest
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
    [JsonPropertyName("transStatus")]
    public long TransStatus { get; set; }
    [JsonPropertyName("transCode")]
    public string TransCode { get; set; }
    [JsonPropertyName("transDate")]
    public long TransDate { get; set; }
    [JsonPropertyName("transAmount")]
    public long TransAmount { get; set; }
    [JsonPropertyName("issuerCode")]
    public string IssuerCode { get; set; }
    [JsonPropertyName("muid")]
    public string Muid { get; set; } 
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    [JsonPropertyName("posId")]
    public string PosId { get; set; }
}