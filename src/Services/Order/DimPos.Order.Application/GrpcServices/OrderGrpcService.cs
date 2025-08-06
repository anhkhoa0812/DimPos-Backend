using DimPos.Order.Application.Common.Protos;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Order.Application.GrpcServices;

public class OrderGrpcService : Common.Protos.OrderGrpcService.OrderGrpcServiceBase
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public OrderGrpcService(IUnitOfWork<OrderContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public override async Task<GetIsNeedToUpdateInventoryByOrderIdResponse> GetIsNeedToUpdateInventoryByOrderId(GetIsNeedToUpdateInventoryByOrderIdRequest request, ServerCallContext context)
    {
        var orderId = Guid.Parse(request.OrderId);
        var storeId = Guid.Parse(request.StoreId);
        
        _logger.Information("BEGIN: GetIsNeedToUpdateInventoryByOrderId for OrderId: {OrderId}, StoreId: {StoreId}", orderId, storeId);
        var order = await _unitOfWork.GetRepository<Domain.Entities.Orders>().SingleOrDefaultAsync(
            predicate: x => x.Id == orderId && x.StoreId == storeId,
            include: x => x.Include(x => x.OrderItems)
        );
        if (order == null)
        {
            _logger.Error("Order not found for OrderId: {OrderId}, StoreId: {StoreId}", orderId, storeId);
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy đơn hàng"));
        }

        return new GetIsNeedToUpdateInventoryByOrderIdResponse()
        {
            OrderId = order.Id.ToString(),
            IsNeedToUpdateInventory = order.IsNeedToUpdateInventory,
            OrderItems =
            {
                order.OrderItems.Select(x => new OrderItemResponse()
                {
                    Id = x.Id.ToString(),
                    ProductVariantId = x.ProductVariantId.ToString(),
                    Quantity = x.Quantity
                })
            }
        };
    }

    public override async Task<GetDetailForCloseFinancialShiftResponse> GetDetailForCloseFinancialShift(GetDetailForCloseFinancialShiftRequest request, ServerCallContext context)
    {
        var financialShiftId = Guid.Parse(request.FinancialShiftId);
        var storeId = Guid.Parse(request.StoreId);

        _logger.Information("BEGIN: GetDetailForCloseFinancialShift for FinancialShiftId: {FinancialShiftId}, StoreId: {StoreId}", financialShiftId, storeId);
        var orders = await _unitOfWork.GetRepository<Orders>().GetListAsync(
            predicate: x => x.FinancialShiftId == financialShiftId && x.StoreId == storeId,
            include: x => x.Include(x => x.OrderItems)
        );
        // var response = new GetDetailForCloseFinancialShiftResponse
        // {
        //     FinancialShiftId = financialShiftId.ToString(),
        //     TotalCashRoundingInShift = 0,
        //     TotalDiscountInShift = 0,
        //     TotalGrossSalesInShift = 0,
        //     TotalTaxInShift = 0,
        //     TotalNetSalesInShift = 0
        // };
        decimal totalCashRoundingInShift = 0;
        decimal totalDiscountInShift = 0;
        decimal totalGrossSalesInShift = 0;
        decimal totalTaxInShift = 0;
        decimal totalNetSalesInShift = 0;
        foreach (var order in orders)
        {
            totalGrossSalesInShift += order.SubTotalAmount;
            totalNetSalesInShift += order.SubTotalAmount - order.DiscountAmount;
            totalTaxInShift += order.TaxAmount;
            totalDiscountInShift += order.DiscountAmount;
            totalCashRoundingInShift += order.AmountPaid - order.TotalAmount;
        }
        
        var response = new GetDetailForCloseFinancialShiftResponse
        {
            FinancialShiftId = financialShiftId.ToString(),
            TotalCashRoundingInShift = (float) totalCashRoundingInShift,
            TotalDiscountInShift = (float) totalDiscountInShift,
            TotalGrossSalesInShift = (float) totalGrossSalesInShift,
            TotalTaxInShift = (float) totalTaxInShift,
            TotalNetSalesInShift = (float) totalNetSalesInShift
        };
        _logger.Information("END: GetDetailForCloseFinancialShift for FinancialShiftId: {FinancialShiftId}, StoreId: {StoreId}", financialShiftId, storeId);
        return response;
    }
}