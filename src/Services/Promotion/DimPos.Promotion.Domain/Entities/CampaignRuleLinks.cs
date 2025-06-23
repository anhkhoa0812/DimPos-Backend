using DimPos.Promotion.Domain.Entities.Common;

namespace DimPos.Promotion.Domain.Entities;

public class CampaignRuleLinks : EntityBase<Guid>
{
    public Guid CampaignId { get; set; }
    public Guid PromotionRuleId { get; set; }
    
    public virtual Campaigns Campaign { get; set; } = null!;
    public virtual PromotionRules PromotionRule { get; set; } = null!;
}