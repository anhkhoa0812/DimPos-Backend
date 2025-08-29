using System.Text.Json.Serialization;

namespace DimPos.Store.Domain.Models.MPos;

public class MPosModel
{
    public long MerchantId { get; set; }
    public string Data { get; set; } = string.Empty;
}
public class MPosModelRequest
{
    public long MerchantId { get; set; }
    [JsonPropertyName("settings")]
    public MPosSettingDetails Settings { get; set; } = new MPosSettingDetails();
}

public class MPosSettingDetails
{
    [JsonPropertyName("secretKey")]
    public string SecretKey { get; set; }
    [JsonPropertyName("muid")]
    public string Muid {get; set;}
    [JsonPropertyName("posId")]
    public string PosId {get; set;}
}