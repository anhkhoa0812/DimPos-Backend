using MassTransit;

namespace SharedProject.Events.Store.CreateStore;

public class CreateStoreAccountErrorModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid BrandAccountId { get; set; }
    public Guid StoreId { get; set; }
    public Guid AccountId { get; set; }
    public string ErrorMessage { get; set; } = String.Empty;
}