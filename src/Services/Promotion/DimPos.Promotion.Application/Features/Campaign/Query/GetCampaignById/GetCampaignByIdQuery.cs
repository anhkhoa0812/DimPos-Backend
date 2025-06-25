using DimPos.Promotion.Domain.Models.Common;
using Mediator;

namespace DimPos.Promotion.Application.Features.Campaign.Query.GetCampaignById;

public class GetCampaignByIdQuery : IRequest<ApiResponse>
{
    public Guid CampaignId { get; set; }
}