using MassTransit;

namespace SharedProject.Events.RemoveMenuForStore;

public class RemoveStorePriceErrorModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid BrandId { get; set; }
    public List<StoreMenuAssignmentsModel> StoreMenuAssignments { get; set; } = new();
}