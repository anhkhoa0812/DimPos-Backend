namespace DimPos.Basket.Application.Models.Request;

public class RemoveCartAppliedPromotionDetailRequest
{
    public List<Guid> PromotionIds { get; set; }
}