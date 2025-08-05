using MassTransit;

namespace SharedProject.Events.Store.CreateStore;

public class CreateStoreAccountRequestModel  : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid BrandAccountId { get; set; }
    public Guid StoreId { get; set; }
    public Guid AccountId { get; set; }
    public string Code { get; set; }
    public string? Email { get; set; }
    public string Username { get; set; }
    public string HashPassword { get; set; }
    public string SaltPassword { get; set; }
}