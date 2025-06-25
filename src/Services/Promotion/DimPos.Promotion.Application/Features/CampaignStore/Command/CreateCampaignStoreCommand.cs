using DimPos.Promotion.Domain.Models.Common;
using Mediator;

namespace DimPos.Promotion.Application.Features.CampaignStore.Command;

public class CreateCampaignStoreCommand : IRequest<ApiResponse>
{
    public Guid CampaignId { get; set; }
    public List<Guid> StoreIds { get; set; }
}

public class CreateCampaignStoreRequest
{
    public List<Guid> StoreIds { get; set; }
}