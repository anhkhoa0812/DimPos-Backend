using DimPos.Notification.Application.Services.Interface;
using DimPos.Notification.Domain.Enums;
using DimPos.Notification.Domain.Models.Request;
using MassTransit;
using SharedProject.Events.Notification;

namespace DimPos.Notification.Application.Consumers;

public class SendNotificationForMultipleAccountRequestConsumer : IConsumer<SendNotificationForMultipleAccountRequestModel>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger _logger;
    
    public SendNotificationForMultipleAccountRequestConsumer(INotificationService notificationService, ILogger logger)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<SendNotificationForMultipleAccountRequestModel> context)
    {
        foreach (var accountRequest in context.Message.Accounts)
        {
            var notificationMessage = new NotificationMessage()
            {
                Message = accountRequest.Message,
                Type = (ENotificationType) accountRequest.Type,
            };
            try
            {
                await _notificationService.SendToAccountAsync(accountRequest.AccountId, notificationMessage);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send notification to account {AccountId}", accountRequest.AccountId);
            }
        }
    }
}