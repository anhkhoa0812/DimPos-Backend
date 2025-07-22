using MassTransit;

namespace SharedProject.Events.UpdateBrandMenuItem;

public class CreateStorePriceForBrandMenuItemRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid BrandId { get; set; }
    public List<Guid> StoreIds { get; set; } = new List<Guid>();
    public List<Guid>? NewProductVariantIds { get; set; } = new List<Guid>();
    public List<Guid>? RemovedProductVariantIds { get; set; } = new List<Guid>();
}