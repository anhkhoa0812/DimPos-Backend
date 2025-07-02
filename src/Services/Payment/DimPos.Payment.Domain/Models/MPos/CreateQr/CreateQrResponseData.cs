using System.Text.Json.Serialization;
using DimPos.Payment.Domain.Enums;

namespace DimPos.Payment.Domain.Models.MPos.CreateQr;

public class CreateQrResponseData
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    [JsonPropertyName("muid")]
    public string Muid { get; set; }
    [JsonPropertyName("merchantId")]
    public long MerchantId { get; set; }
    [JsonPropertyName("udid")]
    public string Udid { get; set; }
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
    [JsonPropertyName("qrType")]
    public string QrType { get; set; }
    [JsonPropertyName("qrId")]
    public string QrId { get; set; }
    [JsonPropertyName("qrCode")]
    public string QrCode { get; set; }
    [JsonPropertyName("expireTime")]
    public long? ExpireTime { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}