using DimPos.Notification.Domain.Entities.Common;
using DimPos.Notification.Domain.Enums;

namespace DimPos.Notification.Domain.Entities;

public class Notifications : EntityAuditBase<Guid>
{
    public ENotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    
    public virtual ICollection<NotificationRecipients> Recipients { get; set; } = new List<NotificationRecipients>();
}