using DimPos.Promotion.Domain.Enums;
using DimPos.Promotion.Domain.Models.Common;
using Mediator;

namespace DimPos.Promotion.Application.Features.Campaign.Command.CreateCampaign;

public class CreateCampaignCommand : IRequest<ApiResponse>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ECampaignChannel Channel { get; set; }
    public int Priority { get; set; }
    public int? MaxTotalUsageLimit { get; set; }
    public int? MaxUsagePerCustomerLimit { get; set; }
    public List<Guid>? PromotionRuleIds { get; set; }
}