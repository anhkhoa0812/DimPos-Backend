using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Enums;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Domain.Models.Response;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using DimPos.Payment.Application.Common.Protos;
using DimPos.Store.Application.Common.Protos;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Order.Application.Features.Dashboards.Query.GetDashboardForBrand;

public class GetDashboardForBrandQueryHandler : IRequestHandler<GetDashboardForBrandQuery, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcService;
    
    public GetDashboardForBrandQueryHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger, IClaimService claimService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService,
        PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
        _paymentGrpcService = paymentGrpcService ?? throw new ArgumentNullException(nameof(paymentGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(GetDashboardForBrandQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy id của thương hiệu");
        }
        if(request.StoreId != null && request.StoreId != Guid.Empty)
        {
            var brandIdGrpcResponse = await _storeGrpcService.GetBrandIdByStoreIdAsync(new GetBrandIdByStoreIdRequest()
            {
                StoreId = request.StoreId.ToString()
            });
            if (!brandIdGrpcResponse.BrandId.Equals(brandIdGrpcResponse.BrandId))
            {
                throw new BadHttpRequestException("Cửa hàng không thuộc về thương hiệu của bạn");
            }
        }
        
        var orders = await _unitOfWork.GetRepository<Orders>().GetListAsync(
            predicate: x => x.BrandId == brandId
                            && (request.StoreId == null || x.StoreId == request.StoreId)
                            && DateOnly.FromDateTime(x.CreatedDate) >= request.FromDate
                            && DateOnly.FromDateTime(x.CreatedDate) <= request.ToDate
                            && x.Status == EOrderStatus.Completed,
            include: x => x.Include(x => x.OrderItems)
        );

        var response = new DashboardResponse()
        {
            TotalDineInRevenue = 0,
            TotalTakeAwayRevenue = 0,
            TotalSubTotalRevenue = 0,
            TotalRevenue = 0,
            TotalDiscountRevenue = 0,
            TotalDineInOrders = 0,
            TotalTakeAwayOrders = 0,
            TotalOrders = 0,
            AverageOrderValue = 0,
            AverageDineInOrderValue = 0,
            AverageTakeAwayOrderValue = 0,
            AverageItemsPerOrder = 0,
            AverageDineInItemsPerOrder = 0,
            AverageTakeAwayItemsPerOrder = 0
        };
        
        var systemPaymentMethods = await _paymentGrpcService.GetAllSystemPaymentMethodsAsync(new GetAllSystemPaymentMethodsRequest());
        var systemPaymentMethodDict = systemPaymentMethods.SystemPaymentMethods.ToDictionary(x => x.PaymentMethod, x => Guid.Parse(x.SystemPaymentMethodId));
        
        if (!orders.Any())
        {
            return new ApiResponse()
            {
                Status = StatusCodes.Status200OK,
                Message = "Lấy dữ liệu thành công",
                Data = response
            };
        }
        
        response.TotalDineInRevenue = orders.Where(x => x.Type == EOrderType.DineIn).Sum(x => x.TotalAmount);
        response.TotalTakeAwayRevenue = orders.Where(x => x.Type == EOrderType.TakeAway).Sum(x => x.TotalAmount);
        response.TotalRevenue = orders.Sum(x => x.TotalAmount);
        response.TotalDiscountRevenue = orders.Sum(x => x.DiscountAmount);
        response.TotalSubTotalRevenue = orders.Sum(x => x.SubTotalAmount);
        response.TotalDineInOrders = orders.Count(x => x.Type == EOrderType.DineIn);
        response.TotalTakeAwayOrders = orders.Count(x => x.Type == EOrderType.TakeAway);
        response.TotalOrders = orders.Count;
        response.AverageOrderValue = response.TotalOrders > 0 ? Math.Round(response.TotalRevenue / response.TotalOrders, 2) : 0;
        response.AverageDineInOrderValue = response.TotalDineInOrders > 0 ? Math.Round(response.TotalDineInRevenue / response.TotalDineInOrders, 2) : 0;
        response.AverageTakeAwayOrderValue = response.TotalTakeAwayOrders > 0 ? Math.Round(response.TotalTakeAwayRevenue / response.TotalTakeAwayOrders, 2) : 0;
        response.AverageItemsPerOrder =  orders.Count > 0 ? (decimal) Math.Round(orders.Average(x => x.OrderItems.Count), 2) : 0;
        response.AverageDineInItemsPerOrder = response.TotalDineInOrders > 0 ? (decimal) Math.Round(orders.Where(x => x.Type == EOrderType.DineIn).Average(x => x.OrderItems.Count), 2) : 0;
        response.AverageTakeAwayItemsPerOrder = response.TotalTakeAwayOrders > 0 ? (decimal)Math.Round(orders.Where(x => x.Type == EOrderType.TakeAway).Average(x => x.OrderItems.Count), 2) : 0;
        response.TotalCashRevenue = orders.Where(x => x.SystemPaymentMethodId == systemPaymentMethodDict[PaymentMethod.Cash]).Sum(x => x.TotalAmount);
        response.TotalQrCodeRevenue = orders.Where(x => x.SystemPaymentMethodId == systemPaymentMethodDict[PaymentMethod.QrVietqr]).Sum(x => x.TotalAmount);
        response.TotalQrEdcRevenue = orders.Where(x => x.SystemPaymentMethodId == systemPaymentMethodDict[PaymentMethod.QrEdc]).Sum(x => x.TotalAmount);
        response.TotalCardEdcRevenue = orders.Where(x => x.SystemPaymentMethodId == systemPaymentMethodDict[PaymentMethod.CardEdc]).Sum(x => x.TotalAmount);
        
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy dữ liệu thành công",
            Data = response
        };
    }
}