using DimPos.Notification.Application.Services.Interface;
using DimPos.Notification.Application.SignalR;
using DimPos.Notification.Domain.Entities;
using DimPos.Notification.Domain.Models.Request;
using DimPos.Notification.Infrastructure.Persistence;
using DimPos.Notification.Infrastructure.Repositories.Interface;
using Microsoft.AspNetCore.SignalR;

namespace DimPos.Notification.Application.Services.Implement;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IUnitOfWork<NotificationContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public NotificationService(IUnitOfWork<NotificationContext> unitOfWork, ILogger logger,
        IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task SendToAccountAsync(Guid accountId, NotificationMessage notificationMessage)
    {
        var accountGroup = $"Account_{accountId}";

        var notification = new Notifications()
        {
            Id = Guid.CreateVersion7(),
            Message = notificationMessage.Message,
            Type = notificationMessage.Type,
        };
        notification.Recipients.Add(new NotificationRecipients()
        {
            Id = Guid.CreateVersion7(),
            NotificationId = notification.Id,
            AccountId = accountId,
            IsRead = false,
            ReadAt = null,
        });
        await _unitOfWork.GetRepository<Notifications>().InsertAsync(notification);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;

        if (!isSuccess)
        {
            _logger.Error("Failed to send notification to account {AccountId}", accountId);
            // throw;
        }
        await _hubContext.Clients.Groups(accountGroup).SendAsync("ReceiveNotification", notificationMessage);
        _logger.Information("Notification sent to account {AccountId} with message: {Message}",
            accountId, notificationMessage.Message);
    }

    public async Task SendToAccountsAsync(List<Guid> accountIds, NotificationMessage notificationMessage)
    {
        var notification = new Notifications()
        {
            Id = Guid.CreateVersion7(),
            Message = notificationMessage.Message,
            Type = notificationMessage.Type,
        };
        var notificationRecipients = accountIds.Select(x => new NotificationRecipients()
        {
            Id = Guid.CreateVersion7(),
            NotificationId = notification.Id,
            AccountId = x,
            IsRead = false,
            ReadAt = null,
        }).ToList();
        notification.Recipients = notificationRecipients;
        await _unitOfWork.GetRepository<Notifications>().InsertAsync(notification);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Failed to send notification to accounts {AccountIds}", string.Join(", ", accountIds));
            // throw;
        }
        var tasks = accountIds.Select(accountId =>
            _hubContext.Clients.Group($"Account_{accountId}")
                .SendAsync("ReceiveNotification", notificationMessage));
        
        await Task.WhenAll(tasks);
        
        _logger.Information("Notification sent to {Count} accounts: {NotificationMessage}", accountIds.Count(), notification.Message);
        

    }
}