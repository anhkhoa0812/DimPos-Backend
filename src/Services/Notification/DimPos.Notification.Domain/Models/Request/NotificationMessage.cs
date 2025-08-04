using DimPos.Notification.Domain.Enums;

namespace DimPos.Notification.Domain.Models.Request;

public class NotificationMessage
{
    public string Message { get; set; } = string.Empty;
    public ENotificationType Type { get; set; }
}