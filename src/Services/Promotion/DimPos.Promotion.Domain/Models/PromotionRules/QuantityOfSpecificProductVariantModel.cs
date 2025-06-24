namespace DimPos.Promotion.Domain.Models.PromotionRules;

public class QuantityOfSpecificProductVariantModel
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
}