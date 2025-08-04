using DimPos.Notification.Domain.Entities;
using DimPos.Notification.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DimPos.Notification.Infrastructure.Persistence.Configurations;

public class NotificationsConfiguration : IEntityTypeConfiguration<Notifications>
{
    public void Configure(EntityTypeBuilder<Notifications> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (ENotificationType)Enum.Parse(typeof(ENotificationType), v)
            );

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(1000);
    }
}