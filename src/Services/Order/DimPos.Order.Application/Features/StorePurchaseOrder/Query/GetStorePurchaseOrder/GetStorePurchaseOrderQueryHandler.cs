using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Domain.Models.Response;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Order.Application.Features.StorePurchaseOrder.Query.GetStorePurchaseOrder;

public class GetStorePurchaseOrderQueryHandler : IRequestHandler<GetStorePurchaseOrderQuery, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetStorePurchaseOrderQueryHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStorePurchaseOrderQuery request, CancellationToken cancellationToken)
    {
        var role = _claimService.GetRole;
        if(!role.Equals("StoreAdmin") && !role.Equals("BrandAdmin"))
            throw new Exception("Bạn không có quyền truy cập vào yêu cầu này.");
        
        if (role.Equals("StoreAdmin"))
        {
            var storeId = _claimService.GetStoreId ?? Guid.Empty;
            if (storeId == Guid.Empty)
                throw new BadHttpRequestException("Không tìm thấy Id của cửa hàng.");

            var response = await _unitOfWork.GetRepository<StorePurchaseOrders>().GetPagingListAsync(
                selector: x => new GetStorePurchaseOrderResponse()
                {
                    Id = x.Id,
                    StoreId = x.StoreId,
                    Status = x.Status,
                    CancellationRequestReasonByStore = x.CancellationRequestReasonByStore,
                    CancellationReasonByBrand = x.CancellationReasonByBrand,
                    NoteFromStore = x.NoteFromStore,
                    NoteFromBrand = x.NoteFromBrand,
                    EstimatedTotalValue = x.EstimatedTotalValue,
                    ConfirmedByBrandAt = x.ConfirmedByBrandAt,
                    CancelledAt = x.CancelledAt,
                    CompletedAt = x.CompletedAt,
                    CreatedByAccountId = x.CreatedByAccountId,
                    CreatedDate = x.CreatedDate,
                    LastModifiedDate = x.LastModifiedDate,
                    StorePurchaseOrderItems = x.StorePurchaseOrderItems.Select(item => new GetStorePurchaseOrderItemByOrderResponse
                    {
                        Id = item.Id,
                        ProductVariantIdSnapshot = item.ProductVariantIdSnapshot,
                        ProductVariantNameSnapshot = item.ProductVariantNameSnapshot,
                        ProductVariantPriceSnapshot = item.ProductVariantPriceSnapshot,
                        TotalPriceOfOrderItems = item.TotalPriceOfOrderItems,
                        Quantity = item.Quantity,
                        ApprovedQuantityByBrand = item.ApprovedQuantityByBrand,
                        ReceivedQuantityByStore = item.ReceivedQuantityByStore
                    }).ToList()
                },
                predicate: x => x.StoreId == storeId,
                page: request.Page,
                size: request.Size,
                sortBy: request.SortBy,
                isAsc: request.IsAsc
            );
            return new ApiResponse
            {
                Status = StatusCodes.Status200OK,
                Message = "Lấy danh sách đơn hàng mua từ cửa hàng thành công.",
                Data = response
            };
        }
        if (role.Equals("BrandAdmin"))
        {
            var brandId = _claimService.GetBrandId ?? Guid.Empty;
            if (brandId == Guid.Empty)
                throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu.");

            var response = await _unitOfWork.GetRepository<StorePurchaseOrders>().GetPagingListAsync(
                selector: x => new GetStorePurchaseOrderResponse()
                {
                    Id = x.Id,
                    StoreId = x.StoreId,
                    Status = x.Status,
                    CancellationRequestReasonByStore = x.CancellationRequestReasonByStore,
                    CancellationReasonByBrand = x.CancellationReasonByBrand,
                    NoteFromStore = x.NoteFromStore,
                    NoteFromBrand = x.NoteFromBrand,
                    EstimatedTotalValue = x.EstimatedTotalValue,
                    ConfirmedByBrandAt = x.ConfirmedByBrandAt,
                    CancelledAt = x.CancelledAt,
                    CompletedAt = x.CompletedAt,
                    CreatedByAccountId = x.CreatedByAccountId,
                    CreatedDate = x.CreatedDate,
                    LastModifiedDate = x.LastModifiedDate,
                    StorePurchaseOrderItems = x.StorePurchaseOrderItems.Select(item => new GetStorePurchaseOrderItemByOrderResponse
                    {
                        Id = item.Id,
                        ProductVariantIdSnapshot = item.ProductVariantIdSnapshot,
                        ProductVariantNameSnapshot = item.ProductVariantNameSnapshot,
                        ProductVariantPriceSnapshot = item.ProductVariantPriceSnapshot,
                        TotalPriceOfOrderItems = item.TotalPriceOfOrderItems,
                        Quantity = item.Quantity,
                        ApprovedQuantityByBrand = item.ApprovedQuantityByBrand,
                        ReceivedQuantityByStore = item.ReceivedQuantityByStore
                    }).ToList()
                },
                predicate: x => x.BrandId == brandId,
                page: request.Page,
                size: request.Size,
                sortBy: request.SortBy,
                isAsc: request.IsAsc
            );
            return new ApiResponse
            {
                Status = StatusCodes.Status200OK,
                Message = "Lấy danh sách đơn hàng mua từ thương hiệu thành công.",
                Data = response
            };
        }
        throw new BadHttpRequestException("Bạn không có quyền truy cập vào yêu cầu này.");
    }
}