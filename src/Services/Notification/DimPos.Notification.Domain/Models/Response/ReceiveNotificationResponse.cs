using DimPos.Notification.Domain.Enums;

namespace DimPos.Notification.Domain.Models.Response;

public class ReceiveNotificationResponse
{
    public string Message { get; set; } = string.Empty;
    public ENotificationType Type { get; set; }
    public int UnReadCount { get; set; }
}