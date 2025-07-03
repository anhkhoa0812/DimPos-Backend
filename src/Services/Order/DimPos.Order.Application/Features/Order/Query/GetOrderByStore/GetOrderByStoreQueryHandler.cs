using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Domain.Models.Response;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Order.Application.Features.Order.Query.GetOrderByStore;

public class GetOrderByStoreQueryHandler : IRequestHandler<GetOrderByStoreQuery, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public GetOrderByStoreQueryHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetOrderByStoreQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin cửa hàng");
        }
        var orders = await _unitOfWork.GetRepository<Orders>().GetPagingListAsync(
            selector: x => new GetOrderByStoreResponse()
            {
                Id = x.Id,
                Type = x.Type,
                Status = x.Status,
                CustomerNameSnapshot = x.CustomerNameSnapshot,
                TotalAmount = x.TotalAmount,
                Note = x.Note,
                CreatedDate = x.CreatedDate,
                CompletedAt = x.CompletedAt,
                TableNumberDineIn = x.TableNumberDineIn,
                PickupTime = x.PickupTime,
                OrderItems = x.OrderItems.Select(oi => new GetOrderItemByStoreResponse()
                {
                    Id = oi.Id,
                    ProductVariantNameSnapshot = oi.ProductVariantNameSnapshot,
                    Quantity = oi.Quantity,
                    UnitPriceSnapshot = oi.UnitPriceSnapshot,
                    TotalPriceBeforeItemDiscount = oi.TotalPriceBeforeItemDiscount,
                    Note = oi.Note
                }).ToList()
            },
            predicate: x => x.StoreId == storeId && 
                            (request.Status == null || x.Status == request.Status) &&
                            (request.Type == null || x.Type == request.Type),
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy,
            isAsc: request.IsAsc
        );
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách đơn hàng thành công",
            Data = orders
        };
    }
}