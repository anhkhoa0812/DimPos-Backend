using DimPos.Promotion.Domain.Enums;

namespace DimPos.Promotion.Domain.Models.Campaigns;

public class GetCampaignByIdResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ECampaignsStatus Status { get; set; }
    public ECampaignChannel Channel { get; set; }
    public int Priority { get; set; }
    public int? MaxTotalUsageLimit { get; set; }
    public int? MaxUsagePerCustomerLimit { get; set; }
    public List<PromotionRulesByGetCampaignByIdResponse>? PromotionRules { get; set; }
    public List<Guid>? StoreIds { get; set; }
}
public class PromotionRulesByGetCampaignByIdResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Priority { get; set; }
}
