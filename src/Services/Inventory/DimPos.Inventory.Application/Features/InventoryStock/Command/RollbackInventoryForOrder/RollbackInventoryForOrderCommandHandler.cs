using DimPos.Inventory.Application.Services.Interface;
using DimPos.Inventory.Domain.Entities;
using DimPos.Inventory.Domain.Enums;
using DimPos.Inventory.Domain.Models.Common;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.Inventory.Infrastructure.Repositories.Interface;
using DimPos.Order.Application.Common.Protos;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Inventory.Application.Features.InventoryStock.Command.RollbackInventoryForOrder;

public class RollbackInventoryForOrderCommandHandler : IRequestHandler<RollbackInventoryForOrderCommand, ApiResponse>
{
    private readonly IUnitOfWork<InventoryContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly OrderGrpcService.OrderGrpcServiceClient _orderGrpcService;
    public RollbackInventoryForOrderCommandHandler(IUnitOfWork<InventoryContext> unitOfWork, ILogger logger,
        IClaimService claimService, OrderGrpcService.OrderGrpcServiceClient orderGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _orderGrpcService = orderGrpcService ?? throw new ArgumentNullException(nameof(orderGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(RollbackInventoryForOrderCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin cửa hàng");
        }

        var orderGrpcResponse = await _orderGrpcService.GetIsNeedToUpdateInventoryByOrderIdAsync(
            new GetIsNeedToUpdateInventoryByOrderIdRequest
            {
                OrderId = request.OrderId.ToString(),
                StoreId = storeId.ToString()
            }, cancellationToken: cancellationToken);
        if (!orderGrpcResponse.IsNeedToUpdateInventory)
        {
            throw new BadHttpRequestException("Đơn hàng không cần cập nhật kho hàng");
        }
        var inventoryTransactions = await _unitOfWork.GetRepository<InventoryTransactions>().GetListAsync(
            predicate: x => x.RelatedOrderId == request.OrderId && 
                            x.Type == EInventoryTransactionType.ConsumptionSale,
            include:  x => x.Include(x => x.InventoryStock));
        if (inventoryTransactions.Any())
        {
            var rollbackTransactions = new List<InventoryTransactions>();
            foreach (var inventoryTransaction in inventoryTransactions)
            {
                inventoryTransaction.InventoryStock.Quantity -= inventoryTransaction.QuantityChange;
                _unitOfWork.GetRepository<Domain.Entities.InventoryStock>()
                    .UpdateAsync(inventoryTransaction.InventoryStock);
                var newInventoryTransaction = new InventoryTransactions()
                {
                    Id = Guid.CreateVersion7(),
                    QuantityChange = -inventoryTransaction.QuantityChange,
                    Type = EInventoryTransactionType.ManualAdjustment,
                    Note = "Hoàn tác kho hàng do hủy đơn hàng",
                    RelatedOrderId = request.OrderId,
                    InventoryStockId = inventoryTransaction.InventoryStockId,
                };
                rollbackTransactions.Add(newInventoryTransaction);
            }

            await _unitOfWork.GetRepository<InventoryTransactions>().InsertRangeAsync(rollbackTransactions);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isSuccess)
            {
                _logger.Error("Failed to rollback inventory transactions for order {OrderId}", request.OrderId);
                throw new Exception("Failed to rollback inventory transactions");
            }
            _logger.Information("Successfully rolled back inventory transactions for order {OrderId}", request.OrderId);
            return new ApiResponse()
            {
                Status = StatusCodes.Status200OK,
                Message = "Hoàn tác kho hàng thành công",
                Data = request.OrderId
            };
        }
        
        _logger.Warning("No inventory transactions found for order {OrderId} to rollback", request.OrderId);
        throw new BadHttpRequestException("Không tìm thấy giao dịch kho hàng để hoàn tác cho đơn hàng");
    }
}