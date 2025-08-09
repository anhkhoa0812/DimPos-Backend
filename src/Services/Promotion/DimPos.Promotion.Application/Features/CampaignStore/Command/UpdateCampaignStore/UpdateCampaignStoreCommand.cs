using DimPos.Promotion.Domain.Models.Common;
using Mediator;

namespace DimPos.Promotion.Application.Features.CampaignStore.Command.UpdateCampaignStore;

public class UpdateCampaignStoreCommand : IRequest<ApiResponse>
{
    public Guid CampaignId { get; set; }
    public List<Guid>? StoreIds { get; set; }
}

public class UpdateCampaignStoreRequest
{
    public List<Guid>? StoreIds { get; set; }
}