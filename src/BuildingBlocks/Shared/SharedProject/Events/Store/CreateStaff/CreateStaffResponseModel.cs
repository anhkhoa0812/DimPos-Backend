using MassTransit;

namespace SharedProject.Events.Store.CreateStaff;

public class CreateStaffResponseModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid StoreAdminAccountId { get; set; }
    public Guid AccountId { get; set; } 
    public Guid StoreId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string HashPassword { get; set; }
    public string SaltPassword { get; set; }
}