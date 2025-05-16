using MassTransit;

namespace SharedProject.Events.AssignMenuForStore;

public class AssignNewStoreMenuResponse : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; }
    public List<Guid> ProductVariantIds { get; set; }
    public List<Guid> StoreIds { get; set; }
    public Guid BrandId { get; set; }
}