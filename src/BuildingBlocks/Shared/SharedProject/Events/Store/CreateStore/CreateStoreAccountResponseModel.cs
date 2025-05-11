using MassTransit;

namespace SharedProject.Events.Store.CreateStore;

public class CreateStoreAccountResponseModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid StoreId { get; set; }
    public Guid AccountId { get; set; }
}