using MassTransit;

namespace SharedProject.Events.RemoveMenuForStore;

public class RemoveStoreMenuModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public List<Guid> ProductVariantIds { get; set; }
    public List<Guid> StoreIds { get; set; }
    public Guid BrandId { get; set; }
    
    public List<StoreMenuAssignmentsModel> StoreMenuAssignments { get; set; } = new();
}