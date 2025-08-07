using DimPos.Notification.Application.Services.Interface;
using DimPos.Notification.Application.SignalR;
using DimPos.Notification.Domain.Entities;
using DimPos.Notification.Domain.Models.Common;
using DimPos.Notification.Domain.Models.Response;
using DimPos.Notification.Infrastructure.Paginate;
using DimPos.Notification.Infrastructure.Persistence;
using DimPos.Notification.Infrastructure.Repositories.Interface;
using DimPos.Notification.Infrastructure.Utils;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Notification.Application.Features.NotificationRecipient.Query.GetNotifications;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, ApiResponse>
{
    private readonly IUnitOfWork<NotificationContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly IHubContext<NotificationHub> _notificationHub;

    public GetNotificationsQueryHandler(IUnitOfWork<NotificationContext> unitOfWork, ILogger logger,
        IClaimService claimService, IHubContext<NotificationHub> notificationHub)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _notificationHub = notificationHub ?? throw new ArgumentNullException(nameof(notificationHub));
    }
    
    public async ValueTask<ApiResponse> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin người dùng hiện tại");
        }

        var notificationRecipients = await _unitOfWork.GetRepository<NotificationRecipients>()
            .GetPagingListAsync(
                predicate: x => x.AccountId == accountId,
                page: request.Page,
                size: request.Size,
                sortBy: request.SortBy ?? "CreatedDate",
                isAsc: request.IsAsc,
                include: x => x.Include(x => x.Notification)
            );
        
        var response = notificationRecipients.Items.Any() ?
            notificationRecipients.Items.Select(x => new GetNotificationsResponse()
            {
                Id = x.Id,
                IsRead = x.IsRead,
                ReadAt = x.ReadAt,
                Notification = new NotificationForGetNotificationsResponse()
                {
                    Id = x.Notification.Id,
                    Message = x.Notification.Message,
                    Type = x.Notification.Type,
                    CreatedDate = x.Notification.CreatedDate,
                    LastModifiedDate = x.Notification.LastModifiedDate
                }
            }).ToList() : new List<GetNotificationsResponse>(); 
        
        if (notificationRecipients.Items.Any())
        {
            foreach (var notificationRecipient in notificationRecipients.Items)
            {
                notificationRecipient.IsRead = true;
                notificationRecipient.ReadAt = TimeUtil.GetCurrentSEATime();
            }
            _unitOfWork.GetRepository<NotificationRecipients>().UpdateRange(notificationRecipients.Items);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isSuccess)
            {
                _logger.Error("Failed to update notification recipients for account {AccountId}", accountId);
                throw new Exception($"Cập nhật đã đọc cho thông báo không thành công");
            }
        }

        var unreadNotificationCount = await _unitOfWork.GetRepository<NotificationRecipients>().GetListAsync(
            predicate: x => x.AccountId == accountId && !x.IsRead,
            selector: x => x.Id
        );
        await _notificationHub.Clients.Group($"Account_{accountId}")
            .SendAsync("ReceiveUnreadNotifications", unreadNotificationCount.Count, cancellationToken: cancellationToken);
        
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách thông báo thành công",
            Data = new Paginate<GetNotificationsResponse>()
            {
                Page = notificationRecipients.Page,
                Size = notificationRecipients.Size,
                Total = notificationRecipients.Total,
                TotalPages = notificationRecipients.TotalPages,
                Items = response
            }
        };
    }
}
