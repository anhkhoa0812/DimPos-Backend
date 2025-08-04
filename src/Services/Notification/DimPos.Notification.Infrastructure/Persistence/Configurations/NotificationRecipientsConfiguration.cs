using DimPos.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Notification.Infrastructure.Persistence.Configurations;

public class NotificationRecipientsConfiguration : IEntityTypeConfiguration<NotificationRecipients>
{
    public void Configure(EntityTypeBuilder<NotificationRecipients> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountId)
            .IsRequired();
        
        builder.Property(x => x.IsRead)
            .IsRequired();
        
        builder.HasOne(x => x.Notification)
            .WithMany(x => x.Recipients)
            .HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}