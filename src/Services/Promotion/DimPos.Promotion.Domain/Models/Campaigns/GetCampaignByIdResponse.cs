using DimPos.Promotion.Domain.Enums;

namespace DimPos.Promotion.Domain.Models.Campaigns;

public class GetCampaignByIdResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public ECampaignChannel Channel { get; set; }
    public int Priority { get; set; }
    public int? MaxTotalUsageLimit { get; set; }
    public int? MaxUsagePerCustomerLimit { get; set; }
    public List<PromotionRulesByGetCampaignByIdResponse>? PromotionRules { get; set; }
    public List<StoreByGetCampaignByIdResponse>? Stores { get; set; } = new List<StoreByGetCampaignByIdResponse>();
}
public class PromotionRulesByGetCampaignByIdResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public RuleActionsResponse RuleActions { get; set; } = new ();
    public List<RuleConditionsResponse> RuleConditions { get; set; } = new();
}
public class StoreByGetCampaignByIdResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public string Address { get; set; } = String.Empty;
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
}
public class RuleActionsResponse
{
    public Guid Id { get; set; }
    public EActionType ActionType { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? TargetCriteriaForItemAction { get; set; } = string.Empty;
    public decimal? MaxDiscountAmountForPercentage { get; set; }
}
public class RuleConditionsResponse
{
    public Guid Id { get; set; }
    public EConditionType ConditionType { get; set; }
    public EOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
}
