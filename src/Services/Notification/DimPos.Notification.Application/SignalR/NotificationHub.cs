using DimPos.Notification.Application.Services.Interface;
using DimPos.Notification.Domain.Entities;
using DimPos.Notification.Infrastructure.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NotificationContext = DimPos.Notification.Infrastructure.Persistence.NotificationContext;

namespace DimPos.Notification.Application.SignalR;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly IUnitOfWork<NotificationContext> _unitOfWork;
    public NotificationHub(ILogger logger, IClaimService claimService, IUnitOfWork<NotificationContext> unitOfWork)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public override async Task OnConnectedAsync()
    {
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            _logger.Warning("User {ContextUserIdentifier} connected without a valid account ID", Context.UserIdentifier);
            return;
        }
        if (accountId != Guid.Empty)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Account_{accountId}");
            _logger.Information("User {ContextUserIdentifier} connected and added to Account_{AccountId} group", 
                Context.UserIdentifier, accountId);
        }

        var unReadNotifications = await _unitOfWork.GetRepository<NotificationRecipients>().GetListAsync(
            predicate: x => x.AccountId == accountId && !x.IsRead
        );
        await Clients.Group($"Account_{accountId}").SendAsync("ReceiveUnreadNotifications", unReadNotifications.Count);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var accountId = _claimService.GetCurrentUserId;
        if (accountId != Guid.Empty)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Account_{accountId}");
            _logger.Information("User {ContextUserIdentifier} disconnected from Account_{AccountId} group", 
                Context.UserIdentifier, accountId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}