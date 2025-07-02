using System.Text.Json.Serialization;

namespace DimPos.Payment.Domain.Models.MPos.Base;

public class MPosRequest
{
    [JsonPropertyName("merchantId")]
    public long MerchantId { get; set; }
    [JsonPropertyName("reqData")]
    public string ReqData { get; set; }
}