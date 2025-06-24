namespace DimPos.Promotion.Domain.Models.PromotionRules;

public class PromotionRulesByCartResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsValid { get; set; }
}