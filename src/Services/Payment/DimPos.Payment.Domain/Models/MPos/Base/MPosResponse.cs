using System.Text.Json.Serialization;

namespace DimPos.Payment.Domain.Models.MPos.Base;

public class MPosResponse
{
    [JsonPropertyName("resData")]
    public string ResData { get; set; }
    [JsonPropertyName("resCode")]
    public int ResCode { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
}