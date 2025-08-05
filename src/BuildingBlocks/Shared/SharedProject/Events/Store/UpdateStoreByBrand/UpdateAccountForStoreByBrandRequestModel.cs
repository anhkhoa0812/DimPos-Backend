using MassTransit;

namespace SharedProject.Events.Store.UpdateStoreByBrand;

public class UpdateAccountForStoreByBrandRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid StoreId { get; set; }
    public Guid BrandAccountId { get; set; }
    public List<Guid> AccountIds { get; set; } = new List<Guid>();
    public StoreStatus Status { get; set; }
}