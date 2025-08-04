using DimPos.Notification.Application.Services.Interface;
using DimPos.Notification.Domain.Entities;
using DimPos.Notification.Domain.Models.Common;
using DimPos.Notification.Infrastructure.Persistence;
using DimPos.Notification.Infrastructure.Repositories.Interface;
using DimPos.Notification.Infrastructure.Utils;
using Mediator;

namespace DimPos.Notification.Application.Features.NotificationRecipient.Command.MakeReadAllNotifications;

public class MakeReadAllNotificationsCommandHandler : IRequestHandler<MakeReadAllNotificationsCommand, ApiResponse>
{
    private readonly IUnitOfWork<NotificationContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public MakeReadAllNotificationsCommandHandler(IUnitOfWork<NotificationContext> unitOfWork, ILogger logger,
        IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    public async ValueTask<ApiResponse> Handle(MakeReadAllNotificationsCommand request, CancellationToken cancellationToken)
    {
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin người dùng hiện tại");
        }
        
        var notificationRecipients = await _unitOfWork.GetRepository<NotificationRecipients>()
            .GetListAsync(predicate: x => x.AccountId == accountId && !x.IsRead);

        if (!notificationRecipients.Any())
        {
            return new ApiResponse()
            {
                Status = StatusCodes.Status200OK,
                Message = "Không có thông báo nào cần đánh dấu đã đọc",
            };
        }
        foreach (var notificationRecipient in notificationRecipients)
        {
            notificationRecipient.IsRead = true;
            notificationRecipient.ReadAt = TimeUtil.GetCurrentSEATime();
        }
        _unitOfWork.GetRepository<NotificationRecipients>().UpdateRange(notificationRecipients);
        var isSuccess = await _unitOfWork.CommitAsync() > 0; 
        if (!isSuccess)
        {
            _logger.Error("Failed to mark all notifications as read for account {AccountId}", accountId);
            throw new Exception("Đánh dấu tất cả thông báo là đã đọc không thành công");
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Đánh dấu tất cả thông báo là đã đọc thành công",
        };
    }
}