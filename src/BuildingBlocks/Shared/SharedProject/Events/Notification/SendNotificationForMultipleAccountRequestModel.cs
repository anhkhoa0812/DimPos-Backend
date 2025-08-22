using MassTransit;

namespace SharedProject.Events.Notification;

public class SendNotificationForMultipleAccountRequestModel : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public List<SendNotificationForAccountRequest> Accounts { get; set; } = new List<SendNotificationForAccountRequest>();
}

public class SendNotificationForAccountRequest
{
    public Guid AccountId { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
}