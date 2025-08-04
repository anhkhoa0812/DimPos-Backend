using DimPos.Notification.Application.Services.Interface;
using DimPos.Notification.Domain.Enums;
using DimPos.Notification.Domain.Models.Request;
using MassTransit;
using SharedProject.Events.Notification;

namespace DimPos.Notification.Application.Consumers;

public class SendNotificationForAccountConsumer : IConsumer<SendNotificationForAccountRequestModel>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger _logger;
    
    public SendNotificationForAccountConsumer(INotificationService notificationService, ILogger logger)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<SendNotificationForAccountRequestModel> context)
    {
        var notificationMessage = new NotificationMessage()
        {
            Message = context.Message.Message,
            Type = (ENotificationType)context.Message.Type
        };
        await _notificationService.SendToAccountAsync(context.Message.AccountId, notificationMessage);
    }
}