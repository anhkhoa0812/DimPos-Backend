using MassTransit;

namespace SharedProject.Events.UpdateBrandPassword;

public class UpdateBrandPasswordRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid AccountId { get; set; }
    public string HashPassword { get; set; } = string.Empty;
    public string SaltPassword { get; set; } = string.Empty;
}