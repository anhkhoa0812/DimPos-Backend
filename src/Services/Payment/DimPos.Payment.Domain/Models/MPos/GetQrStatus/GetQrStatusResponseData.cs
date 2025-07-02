using System.Text.Json.Serialization;

namespace DimPos.Payment.Domain.Models.MPos.GetQrStatus;

public class GetQrStatusResponseData
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    [JsonPropertyName("muid")]
    public string Muid { get; set; }
    [JsonPropertyName("amount")]
    public long Amount { get; set; }
    [JsonPropertyName("transStatus")]
    public int TransStatus { get; set; }
    [JsonPropertyName("qrType")]
    public string QrType { get; set; }
    [JsonPropertyName("paymentIdentifier")]
    public string? PaymentIdentifier { get; set; }
    [JsonPropertyName("transDate")]
    public long? TransDate { get; set; }
}
