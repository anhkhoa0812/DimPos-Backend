using MassTransit;

namespace SharedProject.Events.Store.UpdateStoreByBrand;

public class UpdateAccountForStoreByBrandErrorModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid BrandAccountId { get; set; }
    public Guid StoreId { get; set; }
    public StoreStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
}