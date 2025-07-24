using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Domain.Models.Response;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using DimPos.Store.Application.Common.Protos;
using Mediator;

namespace DimPos.Order.Application.Features.StorePurchaseOrder.Query.GetStorePurchaseOrder;

public class GetStorePurchaseOrderQueryHandler : IRequestHandler<GetStorePurchaseOrderQuery, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    
    public GetStorePurchaseOrderQueryHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger, IClaimService claimService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetStorePurchaseOrderQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        
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
                    RequestedQuantity = item.RequestedQuantity,
                    ApprovedQuantityByBrand = item.ApprovedQuantityByBrand,
                }).ToList()
            },
            predicate: x => (storeId == Guid.Empty || x.StoreId == storeId) &&
                            (brandId == Guid.Empty || x.BrandId == brandId),
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "CreatedDate",
            isAsc: request.IsAsc
        );
        
        var storeIds = response.Items.Select(x => x.StoreId.ToString()).Distinct().ToList();
        var stores = await _storeGrpcService.GetListStoreByStoreIdsAsync(new GetListStoreByStoreIdsRequest()
        {
            StoreIds = { storeIds }
        });
        foreach (var item in response.Items)
        {
            var store = stores.Stores.FirstOrDefault(s => s.Id == item.StoreId.ToString());
            if (store != null)
            {
                item.Store = new StoreForPurchaseOrderResponse
                {
                    Id = Guid.Parse(store.Id),
                    Name = store.Name,
                    Phone = store.Phone,
                    Email = store.Email,
                    Description = store.Description,
                    Address = store.Address,
                    Latitude = store.Latitude,
                    Longitude = store.Longitude
                };
            }
        }
        return new ApiResponse
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách đơn hàng mua từ thương hiệu thành công.",
            Data = response
        };
    }
}