using DimPos.Promotion.Domain.Entities.Common;

namespace DimPos.Promotion.Domain.Entities;

public class PromotionRules : EntityAuditBase<Guid>
{
    public Guid BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    
    public virtual ICollection<CampaignRuleLinks> CampaignRuleLinks { get; set; } = new List<CampaignRuleLinks>();
    public virtual RuleActions RuleActions { get; set; } = new RuleActions();
    public virtual ICollection<RuleConditions> RuleConditions { get; set; } = new List<RuleConditions>();
}