using DimPos.Promotion.Domain.Models.Common;
using Mediator;

namespace DimPos.Promotion.Application.Features.Campaign.Command.UpdateCampaign;

public class UpdateCampaignCommand  : IRequest<ApiResponse>
{
    public Guid CampaignId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Priority { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UpdateCampaignRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Priority { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}