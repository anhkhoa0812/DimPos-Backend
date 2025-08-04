using DimPos.Notification.Application.Services.Interface;
using DimPos.Notification.Domain.Entities;
using DimPos.Notification.Domain.Models.Common;
using DimPos.Notification.Infrastructure.Persistence;
using DimPos.Notification.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Notification.Application.Features.NotificationRecipient.Command.RemoveAllNotifications;

public class RemoveAllNotificationsCommandHandler : IRequestHandler<RemoveAllNotificationsCommand, ApiResponse>
{
    private readonly IUnitOfWork<NotificationContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public RemoveAllNotificationsCommandHandler(IUnitOfWork<NotificationContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    public async ValueTask<ApiResponse> Handle(RemoveAllNotificationsCommand request, CancellationToken cancellationToken)
    {
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin người dùng hiện tại");
        }
        
        var notificationRecipients = await _unitOfWork.GetRepository<NotificationRecipients>()
            .GetListAsync(predicate: x => x.AccountId == accountId);

        if (!notificationRecipients.Any())
        {
            return new ApiResponse()
            {
                Status = StatusCodes.Status200OK,
                Message = "Không có thông báo nào để xóa",
            };
        }
        
        _unitOfWork.GetRepository<NotificationRecipients>().DeleteRangeAsync(notificationRecipients);

        var notifications = await _unitOfWork.GetRepository<Notifications>().GetListAsync(
            predicate: x => x.Recipients == null || !x.Recipients.Any()
        );
        if (notifications.Any())
        {
            _unitOfWork.GetRepository<Notifications>().DeleteRangeAsync(notifications);
        }
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Failed to remove all notifications for account {AccountId}", accountId);
            throw new Exception("Xóa tất cả thông báo không thành công");
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Xóa tất cả thông báo thành công",
        };
    }
}