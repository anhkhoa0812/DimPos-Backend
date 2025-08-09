using DimPos.Promotion.Domain.Entities.Common;

namespace DimPos.Promotion.Domain.Entities;

public class CampaignStores : EntityBase<Guid>
{
    public Guid CampaignId { get; set; }
    public Guid StoreId { get; set; }
    public virtual Campaigns Campaign { get; set; }
}