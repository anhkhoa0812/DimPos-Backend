using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Domain.Models.Response;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Order.Application.Features.Order.Query.GetOrderWithId;

public class GetOrderWithIdQueryHandler : IRequestHandler<GetOrderWithIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public GetOrderWithIdQueryHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetOrderWithIdQuery request, CancellationToken cancellationToken)
    {
        var storeId  = _claimService.GetStoreId ?? Guid.Empty;
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        var order = await _unitOfWork.GetRepository<Orders>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.OrderId 
                            && (storeId == Guid.Empty || x.StoreId == storeId) 
                            && (brandId == Guid.Empty || x.BrandId == brandId),
            include: x => x.Include(x => x.OrderItems)
                .ThenInclude(x => x.OrderItemSelectedOptions)
                .Include(x => x.AppliedOrderPromotions)
                .Include(x => x.AppliedTax)
        );
        if (order == null)
        {
            throw new BadHttpRequestException("Không tìm thấy đơn hàng");
        }

        var response = new GetOrderWithIdByStoreResponse()
        {
            Id = order.Id,
            Type = order.Type,
            Status = order.Status,
            CustomerNameSnapshot = order.CustomerNameSnapshot,
            SubTotalAmount = order.SubTotalAmount,
            DiscountAmount = order.DiscountAmount,
            TaxAmount = order.TaxAmount,
            TotalAmount = order.TotalAmount,
            AmountPaid = order.AmountPaid,
            CashRoundingAmount = order.CashRoundingAmount,
            PickupTime = order.PickupTime,
            Note = order.Note,
            SystemPaymentMethodNameSnapshot = order.SystemPaymentMethodNameSnapshot,
            IsNeedToUpdateInventory = order.IsNeedToUpdateInventory,
            CreatedDate = order.CreatedDate,
            TableNumberDineIn = order.TableNumberDineIn,
            OrderItems = order.OrderItems.Select(oi => new GetOrderItemsByOrderByIdResponse()
            {
                Id = oi.Id,
                ProductNameSnapshot = oi.ProductNameSnapshot,
                ProductVariantNameSnapshot = oi.ProductVariantNameSnapshot,
                Quantity = oi.Quantity,
                UnitPriceSnapshot = oi.UnitPriceSnapshot,
                TotalPriceBeforeItemDiscount = oi.TotalPriceBeforeItemDiscount,
                Note = oi.Note,
                OrderItemSelectedOptions = oi.OrderItemSelectedOptions?.Select(x => new GetOrderItemSelectedOptionsByOrderIdResponse()
                {
                    Id = x.Id,
                    ModifierGroupId = x.ModifierGroupId,
                    ModifierOptionId = x.ModifierOptionId,
                    ModifierGroupSnapshot = x.ModifierGroupSnapshot,
                    ModifierOptionSnapshot = x.ModifierOptionSnapshot,
                    PriceDeltaOptionSnapshot = x.PriceDeltaOptionSnapshot
                }).ToList()
            }).ToList(),
            AppliedOrderPromotions = order.AppliedOrderPromotions?.Select(aop =>
                new GetAppliedOrderPromotionsByOrderIdResponse()
                {
                    Id = aop.Id,
                    PromotionNameSnapshot = aop.PromotionNameSnapshot,
                    PromotionTypeSnapshot = aop.PromotionTypeSnapshot,
                    PromotionDescriptionSnapshot = aop.PromotionDescriptionSnapshot,
                    DiscountAmountApplied = aop.DiscountAmountApplied
                }).ToList()
        };
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy thông tin đơn hàng thành công",
            Data = response
        };
    }
}