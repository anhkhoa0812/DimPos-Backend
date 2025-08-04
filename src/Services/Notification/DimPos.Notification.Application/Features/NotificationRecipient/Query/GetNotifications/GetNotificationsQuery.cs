using DimPos.Notification.Domain.Models.Common;
using Mediator;

namespace DimPos.Notification.Application.Features.NotificationRecipient.Query.GetNotifications;

public class GetNotificationsQuery : IRequest<ApiResponse>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; }
}