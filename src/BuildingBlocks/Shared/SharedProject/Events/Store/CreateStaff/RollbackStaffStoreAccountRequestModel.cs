using MassTransit;

namespace SharedProject.Events.Store.CreateStaff;

public class RollbackStaffStoreAccountRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid StoreId { get; set; }
    public Guid AccountId { get; set; }
}