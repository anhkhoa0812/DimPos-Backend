using DimPos.Promotion.Domain.Entities.Common;
using DimPos.Promotion.Domain.Enums;

namespace DimPos.Promotion.Domain.Entities;

public class Campaigns : EntityAuditBase<Guid>
{
    public Guid BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public ECampaignChannel Channel { get; set; }
    public int Priority { get; set; }
    public int? MaxTotalUsageLimit { get; set; }
    public int? MaxUsagePerCustomerLimit { get; set; }
    
    public virtual ICollection<CampaignStores>? CampaignStores { get; set; }
    public virtual ICollection<CampaignRuleLinks>? CampaignRuleLinks { get; set; }
}