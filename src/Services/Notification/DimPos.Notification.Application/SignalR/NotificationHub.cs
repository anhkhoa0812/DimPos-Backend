using DimPos.Notification.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DimPos.Notification.Application.SignalR;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public NotificationHub(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task OnConnectedAsync()
    {
        var accountId = _claimService.GetCurrentUserId;
        if (accountId != Guid.Empty)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Account_{accountId}");
            _logger.Information("User {ContextUserIdentifier} connected and added to Account_{AccountId} group", 
                Context.UserIdentifier, accountId);
        }

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