using MassTransit;

namespace SharedProject.Events.Store.UpdateStoreByBrand;

public class UpdateAccountForStoreByBrandResponseModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid StoreId { get; set; }
    public StoreStatus Status { get; set; }
}