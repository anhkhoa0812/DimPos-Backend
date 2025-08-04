using MassTransit;

namespace SharedProject.Events.Store.CreateStaff;

public class CreateStaffAccountErrorModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid StoreAdminAccountId { get; set; }
    public Guid StoreId { get; set; }
    public Guid AccountId { get; set; }
    public string Message { get; set; } = string.Empty;
}