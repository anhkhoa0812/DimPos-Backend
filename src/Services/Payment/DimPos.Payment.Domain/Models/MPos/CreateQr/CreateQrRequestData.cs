using System.Text.Json.Serialization;
using DimPos.Payment.Domain.Enums;

namespace DimPos.Payment.Domain.Models.MPos.CreateQr;

public class CreateQrRequestData
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    [JsonPropertyName("muid")]
    public string Muid { get; set; }
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
    [JsonPropertyName("qrType")]
    public string QrType { get; set; }
    [JsonPropertyName("Description")]
    public string? Description { get; set; }
}