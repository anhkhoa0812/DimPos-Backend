using MassTransit;

namespace SharedProject.Events.Notification;

public class SendNotificationForAccountRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid AccountId { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
}
