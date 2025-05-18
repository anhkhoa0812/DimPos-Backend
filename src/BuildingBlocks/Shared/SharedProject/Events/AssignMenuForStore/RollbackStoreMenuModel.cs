using MassTransit;

namespace SharedProject.Events.AssignMenuForStore;

public class RollbackStoreMenuModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid BrandId { get; set; }
    public Guid BrandMenuId { get; set; }
    public List<Guid> StoreIds { get; set; } = new List<Guid>();
}