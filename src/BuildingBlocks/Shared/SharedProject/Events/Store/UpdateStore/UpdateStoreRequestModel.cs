using MassTransit;

namespace SharedProject.Events.Store.UpdateStore;

public class UpdateStoreRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid AccountId { get; set; }
    public string HashPassword { get; set; } = String.Empty;
    public string SaltPassword { get; set; } = String.Empty;
}