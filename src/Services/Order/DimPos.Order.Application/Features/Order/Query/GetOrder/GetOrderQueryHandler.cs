using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Domain.Models.Response;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Order.Application.Features.Order.Query.GetOrder;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;

    public GetOrderQueryHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
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
            predicate: x => (storeId == Guid.Empty || x.StoreId == storeId) && 
                            (brandId == Guid.Empty || x.BrandId == brandId) &&
                            (request.Status == null || x.Status == request.Status) &&
                            (request.Type == null || x.Type == request.Type) &&
                            (request.FromDate == null || x.CreatedDate >= request.FromDate) && 
                            (request.ToDate == null || x.CreatedDate <= request.ToDate),
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "CreatedDate",
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