using MassTransit;

namespace SharedProject.Events.AssignMenuForStore;

public class AssignNewStoreMenuModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public List<Guid> ProductVariantIds { get; set; }
    public List<Guid> StoreIds { get; set; }
    public Guid BrandId { get; set; }
    public Guid BrandMenuId { get; set; }
}