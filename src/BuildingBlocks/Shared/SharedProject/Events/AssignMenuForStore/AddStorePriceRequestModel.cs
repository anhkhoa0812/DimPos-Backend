using MassTransit;

namespace SharedProject.Events.AssignMenuForStore;

public class AddStorePriceRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid BrandId { get; set; }
    public Guid BrandMenuId { get; set; }
    public List<StorePriceRequest> StorePrices { get; set; } = new List<StorePriceRequest>();
}
public class StorePriceRequest
{
    public Guid StoreId { get; set; }
    public Guid ProductVariantId { get; set; }
}