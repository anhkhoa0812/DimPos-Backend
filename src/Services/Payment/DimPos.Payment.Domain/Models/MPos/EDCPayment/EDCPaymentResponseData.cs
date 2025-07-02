using System.Text.Json.Serialization;
using DimPos.Payment.Domain.Enums;

namespace DimPos.Payment.Domain.Models.MPos.EDCPayment;

public class EDCPaymentResponseData
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    [JsonPropertyName("posId")]
    public string PosId { get; set; }
    [JsonPropertyName("merchantId")]
    public long MerchantId { get; set; }
    [JsonPropertyName("amount")]
    public long Amount { get; set; }
    [JsonPropertyName("paymentMethod")]
    public string PaymentMethod { get; set; }
}