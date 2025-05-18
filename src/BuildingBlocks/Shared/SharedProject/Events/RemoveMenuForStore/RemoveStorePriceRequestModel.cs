using MassTransit;

namespace SharedProject.Events.RemoveMenuForStore;

public class RemoveStorePriceRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid BrandId { get; set; }
    public List<StorePriceRequest> StorePrices { get; set; } = new();
    public List<StoreMenuAssignmentsModel> StoreMenuAssignments { get; set; } = new();
}
public class StorePriceRequest
{
    public Guid StoreId { get; set; }
    public Guid ProductVariantId { get; set; }
}