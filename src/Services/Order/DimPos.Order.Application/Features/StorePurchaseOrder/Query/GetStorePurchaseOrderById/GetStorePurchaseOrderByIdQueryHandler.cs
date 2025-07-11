using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Domain.Models.Response;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Order.Application.Features.StorePurchaseOrder.Query.GetStorePurchaseOrderById;

public class GetStorePurchaseOrderByIdQueryHandler : IRequestHandler<GetStorePurchaseOrderByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetStorePurchaseOrderByIdQueryHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStorePurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var role = _claimService.GetRole;
        StorePurchaseOrders? storePurchaseOrder;
        if (role.Equals("StoreAdmin"))
        {
            var storeId = _claimService.GetStoreId ?? Guid.Empty;
            if(storeId == Guid.Empty) 
                throw new BadHttpRequestException("Không tìm thấy Id của cửa hàng.");

            storePurchaseOrder = await _unitOfWork.GetRepository<StorePurchaseOrders>().SingleOrDefaultAsync(
                predicate: x => x.Id == request.StorePurchaseOrderId && x.StoreId == storeId,
                include: x => x.Include(y => y.StorePurchaseOrderItems)
            );
        }
        else if (role.Equals("BrandAdmin"))
        {
            var brandId = _claimService.GetBrandId ?? Guid.Empty;
            if(brandId == Guid.Empty) 
                throw new BadHttpRequestException("Không tìm thấy Id của thương hiệu.");

            storePurchaseOrder = await _unitOfWork.GetRepository<StorePurchaseOrders>().SingleOrDefaultAsync(
                predicate: x => x.Id == request.StorePurchaseOrderId && x.BrandId == brandId,
                include: x => x.Include(y => y.StorePurchaseOrderItems)
            );
        }
        else
        {
            throw new Exception("Bạn không có quyền truy cập vào yêu cầu này.");
        }
        if (storePurchaseOrder == null)
            throw new BadHttpRequestException("Không tìm thấy đơn hàng mua sắm của cửa hàng.");
        
        var response = new GetStorePurchaseOrderResponse()
        {
            Id = storePurchaseOrder.Id,
            StoreId = storePurchaseOrder.StoreId,
            Status = storePurchaseOrder.Status,
            CancellationRequestReasonByStore = storePurchaseOrder.CancellationRequestReasonByStore,
            CancellationReasonByBrand = storePurchaseOrder.CancellationReasonByBrand,
            NoteFromStore = storePurchaseOrder.NoteFromStore,
            NoteFromBrand = storePurchaseOrder.NoteFromBrand,
            EstimatedTotalValue = storePurchaseOrder.EstimatedTotalValue,
            ConfirmedByBrandAt = storePurchaseOrder.ConfirmedByBrandAt,
            CancelledAt = storePurchaseOrder.CancelledAt,
            CompletedAt = storePurchaseOrder.CompletedAt,
            CreatedByAccountId = storePurchaseOrder.CreatedByAccountId,
            CreatedDate = storePurchaseOrder.CreatedDate,
            LastModifiedDate = storePurchaseOrder.LastModifiedDate,
            StorePurchaseOrderItems = storePurchaseOrder.StorePurchaseOrderItems.Select(item => new GetStorePurchaseOrderItemByOrderResponse
            {
                Id = item.Id,
                ProductVariantIdSnapshot = item.ProductVariantIdSnapshot,
                ProductVariantNameSnapshot = item.ProductVariantNameSnapshot,
                ProductVariantPriceSnapshot = item.ProductVariantPriceSnapshot,
                TotalPriceOfOrderItems = item.TotalPriceOfOrderItems,
                RequestedQuantity = item.RequestedQuantity,
                ApprovedQuantityByBrand = item.ApprovedQuantityByBrand
            }).ToList()
        };
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy thông tin đơn hàng mua sắm của cửa hàng thành công.",
            Data = response
        };
    }
}