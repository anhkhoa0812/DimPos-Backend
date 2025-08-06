using Confluent.Kafka;
using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Enums;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using DimPos.Order.Infrastructure.Utils;
using MassTransit;
using Mediator;
using Microsoft.EntityFrameworkCore;
using SharedProject.Events.UpdateInventoryForInternalOrder;

namespace DimPos.Order.Application.Features.StorePurchaseOrder.Command.UpdateStorePurchaseOrder;

public class UpdateStorePurchaseOrderCommandHandler : IRequestHandler<UpdateStorePurchaseOrderCommand, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly ITopicProducer<Null, InternalOrderDoneByStoreResponseModel> _producer;
    
    public UpdateStorePurchaseOrderCommandHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger, IClaimService claimService,
        ITopicProducer<Null, InternalOrderDoneByStoreResponseModel> producer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateStorePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy Id của tài khoản");
        }
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        var role = _claimService.GetRole;
        var storePurchaseOrder = await _unitOfWork.GetRepository<StorePurchaseOrders>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.StorePurchaseOrderId &&
                            (storeId == Guid.Empty || x.StoreId == storeId) &&
                            (brandId == Guid.Empty || x.BrandId == brandId),
            include: x => x.Include(x => x.StorePurchaseOrderItems)
        );
        if (storePurchaseOrder == null)
            throw new BadHttpRequestException("Không tìm thấy đơn hàng mua sắm của cửa hàng");

        switch (request.Status)
        {
            case EStorePurchaseOrderStatus.New:
                throw new BadHttpRequestException("Không thể cập nhật trạng thái đơn hàng mua sắm của cửa hàng sang trạng thái mới");
            case EStorePurchaseOrderStatus.ErrorWhenUpdatingInventory:
                throw new BadHttpRequestException("Không thể cập nhật trạng thái đơn hàng mua sắm của cửa hàng sang trạng thái lỗi khi cập nhật kho");
            case EStorePurchaseOrderStatus.BrandConfirmed:
                if (!role.Equals("BrandAdmin"))
                {
                    throw new BadHttpRequestException("Không có quyền cập nhật đơn hàng mua sắm của cửa hàng");
                }
                if (request.StorePurchaseOrderItemRequests == null)
                {
                    throw new BadHttpRequestException("Danh sách mặt hàng mua sắm của cửa hàng không được để trống");
                }
                if (request.StorePurchaseOrderItemRequests.Count != storePurchaseOrder.StorePurchaseOrderItems.Count)
                {
                    throw new BadHttpRequestException("Danh sách mặt hàng mua sắm của cửa hàng không hợp lệ");
                }
                if (storePurchaseOrder.Status != EStorePurchaseOrderStatus.New)
                {
                    throw new BadHttpRequestException("Trạng thái đơn hàng mua sắm của cửa hàng không hợp lệ");
                }
                foreach (var storePurchaseOrderItem in storePurchaseOrder.StorePurchaseOrderItems)
                {
                    var requestStorePurchaseOrderItem = request.StorePurchaseOrderItemRequests
                        .FirstOrDefault(x => x.Id == storePurchaseOrderItem.Id);
                    if (requestStorePurchaseOrderItem == null)
                    {
                        throw new BadHttpRequestException($"Không tìm thấy mặt hàng mua sắm của cửa hàng với Id {storePurchaseOrderItem.Id}");
                    }
                    if(requestStorePurchaseOrderItem.ApprovedQuantityByBrand > storePurchaseOrderItem.RequestedQuantity)
                    {
                        throw new BadHttpRequestException($"Số lượng được phê duyệt của mặt hàng mua sắm của cửa hàng với Id {storePurchaseOrderItem.Id} không được lớn hơn số lượng yêu cầu");
                    }
                    storePurchaseOrderItem.ApprovedQuantityByBrand =
                        requestStorePurchaseOrderItem.ApprovedQuantityByBrand;
                    storePurchaseOrderItem.TotalPriceOfOrderItems =
                        storePurchaseOrderItem.ApprovedQuantityByBrand.Value *
                        storePurchaseOrderItem.ProductVariantPriceSnapshot;
                }
                storePurchaseOrder.EstimatedTotalValue = storePurchaseOrder.StorePurchaseOrderItems
                    .Sum(x => x.TotalPriceOfOrderItems);
                storePurchaseOrder.Status = EStorePurchaseOrderStatus.BrandConfirmed;
                storePurchaseOrder.ConfirmedByBrandAt = TimeUtil.GetCurrentSEATime();
                break;
            case EStorePurchaseOrderStatus.CancelledByStore:
                if (!role.Equals("StoreAdmin"))
                {
                    throw new BadHttpRequestException("Không có quyền cập nhật đơn hàng mua sắm của cửa hàng");
                }
                if (string.IsNullOrEmpty(request.CancellationRequestReasonByStore))
                {
                    throw new BadHttpRequestException("Lý do hủy đơn hàng không được để trống");
                }
                if (storePurchaseOrder.Status != EStorePurchaseOrderStatus.New)
                {
                    throw new BadHttpRequestException("Trạng thái đơn hàng mua sắm của cửa hàng không hợp lệ");
                }
                storePurchaseOrder.Status = EStorePurchaseOrderStatus.CancelledByStore;
                storePurchaseOrder.CancellationRequestReasonByStore = request.CancellationRequestReasonByStore;
                storePurchaseOrder.CancelledAt = TimeUtil.GetCurrentSEATime();
                break;
            case EStorePurchaseOrderStatus.RejectedByBrand:
                if (!role.Equals("BrandAdmin"))
                {
                    throw new BadHttpRequestException("Không có quyền cập nhật đơn hàng mua sắm của cửa hàng");
                }
                if (string.IsNullOrEmpty(request.CancellationReasonByBrand))
                {
                    throw new BadHttpRequestException("Lý do hủy đơn hàng không được để trống");
                }

                if (storePurchaseOrder.Status != EStorePurchaseOrderStatus.New)
                {
                    throw new BadHttpRequestException("Trạng thái đơn hàng mua sắm của cửa hàng không hợp lệ");
                }
                storePurchaseOrder.Status = EStorePurchaseOrderStatus.RejectedByBrand;
                storePurchaseOrder.CancellationReasonByBrand = request.CancellationReasonByBrand;
                storePurchaseOrder.CancelledAt = TimeUtil.GetCurrentSEATime();
                break;
            case EStorePurchaseOrderStatus.CancelledByBrand:
                if (!role.Equals("BrandAdmin"))
                {
                    throw new BadHttpRequestException("Không có quyền cập nhật đơn hàng mua sắm của cửa hàng");
                }
                if (string.IsNullOrEmpty(request.CancellationReasonByBrand))
                {
                    throw new BadHttpRequestException("Lý do hủy đơn hàng không được để trống");
                }
                if (storePurchaseOrder.Status != EStorePurchaseOrderStatus.BrandConfirmed)
                {
                    throw new BadHttpRequestException("Trạng thái đơn hàng mua sắm của cửa hàng không hợp lệ");
                }
                storePurchaseOrder.Status = EStorePurchaseOrderStatus.CancelledByBrand;
                storePurchaseOrder.CancellationReasonByBrand = request.CancellationReasonByBrand;
                storePurchaseOrder.CancelledAt = TimeUtil.GetCurrentSEATime();
                break;
            case EStorePurchaseOrderStatus.DoneByStore:
                if (!role.Equals("StoreAdmin"))
                {
                    throw new BadHttpRequestException("Không có quyền cập nhật đơn hàng mua sắm của cửa hàng");
                }
                if (storePurchaseOrder.Status != EStorePurchaseOrderStatus.BrandConfirmed)
                {
                    throw new BadHttpRequestException("Trạng thái đơn hàng mua sắm của cửa hàng không hợp lệ");
                }
                
                storePurchaseOrder.Status = EStorePurchaseOrderStatus.DoneByStore;
                storePurchaseOrder.CompletedAt = TimeUtil.GetCurrentSEATime();
                break;
            default:
                throw new BadHttpRequestException("Trạng thái đơn hàng mua sắm của cửa hàng không hợp lệ");
        }
        _unitOfWork.GetRepository<StorePurchaseOrders>().UpdateAsync(storePurchaseOrder);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new BadHttpRequestException("Cập nhật đơn hàng mua sắm của cửa hàng không thành công");
        }

        if (request.Status == EStorePurchaseOrderStatus.DoneByStore)
        {
            var doneByStoreResponseModel = new InternalOrderDoneByStoreResponseModel()
            {
                CorrelationId = Guid.CreateVersion7(),
                AccountId = accountId,
                StoreId = storePurchaseOrder.StoreId,
                StorePurchaseOrderId = storePurchaseOrder.Id,
                StorePurchaseOrderItems = storePurchaseOrder.StorePurchaseOrderItems.Select(x =>
                    new StorePurchaseOrderItemRequestModel()
                    {
                        ProductVariantId = x.ProductVariantIdSnapshot,
                        ApprovedQuantityByBrand = x.ApprovedQuantityByBrand.Value
                    }).ToList()
            };
            await _producer.Produce(
                key: null,
                doneByStoreResponseModel,
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật đơn hàng mua sắm của cửa hàng thành công",
        };
    }
}