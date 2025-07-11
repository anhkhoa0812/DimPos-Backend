using System.Text.Json.Serialization;

namespace DimPos.Promotion.Domain.Models.PromotionRules;

public class QuantityOfSpecificProductVariantModel
{
    [JsonPropertyName("productVariantId")]
    public Guid ProductVariantId { get; set; }
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}