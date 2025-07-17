using MassTransit;

namespace SharedProject.Events.Store.UpdateStaff;

public class UpdateStaffRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid StaffId { get; set; }
    public string? Code { get; set; }
    public string? Username { get; set; }
    public string? HashPassword { get; set; } 
    public string? SaltPassword { get; set; }
    public string? Email { get; set; }
    public EAccountStatusForEvent? Status { get; set; }
}
public enum EAccountStatusForEvent
{
    Active,
    Inactive,
    Suspended,
    Deleted
}