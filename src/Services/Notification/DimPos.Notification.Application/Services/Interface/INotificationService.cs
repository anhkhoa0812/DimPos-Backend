using DimPos.Notification.Domain.Models.Request;

namespace DimPos.Notification.Application.Services.Interface;

public interface INotificationService
{
    Task SendToAccountAsync(Guid accountId, NotificationMessage notificationMessage);
    Task SendToAccountsAsync(List<Guid> accountIds, NotificationMessage notificationMessage);
}