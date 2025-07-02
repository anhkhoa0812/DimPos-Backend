using System.Text.Json.Serialization;

namespace DimPos.Payment.Domain.Models.MPos.RefundEDCPayment;

public class RefundEDCPaymentResponseData
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
    [JsonPropertyName("transCode")]
    public string TransCode { get; set; }
    [JsonPropertyName("refundAmount")]
    public long RefundAmount { get; set; }
    [JsonPropertyName("restedAmount")]
    public long RestedAmount { get; set; }
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; }
}