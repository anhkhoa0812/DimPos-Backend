using System.Text.Json.Serialization;

namespace DimPos.Payment.Domain.Models.MPos.RefundEDCPayment;

public class RefundEDCPaymentRequestData
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
    [JsonPropertyName("transCode")]
    public string TransCode { get; set; }
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } 
    [JsonPropertyName("posId")]
    public string PosId { get; set; }
    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }
    [JsonPropertyName("refundAmount")]
    public decimal RefundAmount { get; set; }
}