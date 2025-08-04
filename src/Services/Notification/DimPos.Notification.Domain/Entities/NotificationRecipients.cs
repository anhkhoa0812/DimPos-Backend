using DimPos.Notification.Domain.Entities.Common;

namespace DimPos.Notification.Domain.Entities;

public class NotificationRecipients : EntityAuditBase<Guid>
{
    public Guid AccountId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public Guid NotificationId { get; set; }
    public virtual Notifications? Notification { get; set; }
}