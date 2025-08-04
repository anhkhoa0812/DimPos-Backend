using DimPos.Notification.Domain.Enums;

namespace DimPos.Notification.Domain.Models.Response;

public class GetNotificationsResponse
{
    public Guid Id { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public NotificationForGetNotificationsResponse Notification { get; set; } = new NotificationForGetNotificationsResponse();
}

public class NotificationForGetNotificationsResponse
{
    public Guid Id { get; set; }
    public ENotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}