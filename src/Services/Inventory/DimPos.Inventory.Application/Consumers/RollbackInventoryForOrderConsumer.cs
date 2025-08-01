using DimPos.Inventory.Domain.Entities;
using DimPos.Inventory.Domain.Enums;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.Inventory.Infrastructure.Repositories.Interface;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedProject.Events.Payment.UpdatePaymentTransaction;

namespace DimPos.Inventory.Application.Consumers;

public class RollbackInventoryForOrderConsumer : IConsumer<RollbackInventoryForOrderRequestModel>
{
    private readonly IUnitOfWork<InventoryContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public RollbackInventoryForOrderConsumer(IUnitOfWork<InventoryContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<RollbackInventoryForOrderRequestModel> context)
    {
        var inventoryTransactions = await _unitOfWork.GetRepository<InventoryTransactions>().GetListAsync(
            predicate: x => x.RelatedOrderId == context.Message.OrderId && 
                            x.Type == EInventoryTransactionType.ConsumptionSale,
            include:  x => x.Include(x => x.InventoryStock)
        );
        if (inventoryTransactions.Any())
        {
            var rollbackTransactions = new List<InventoryTransactions>();
            foreach (var inventoryTransaction in inventoryTransactions)
            {
                inventoryTransaction.InventoryStock.Quantity -= inventoryTransaction.QuantityChange;
                _unitOfWork.GetRepository<InventoryStock>().UpdateAsync(inventoryTransaction.InventoryStock);
                var newInventoryTransaction = new InventoryTransactions()
                {
                    Id = Guid.CreateVersion7(),
                    QuantityChange = -inventoryTransaction.QuantityChange,
                    Type = EInventoryTransactionType.ManualAdjustment,
                    Note = "Hoàn tác kho hàng do hủy đơn hàng",
                    RelatedOrderId = context.Message.OrderId,
                    InventoryStockId = inventoryTransaction.InventoryStockId,
                };
                rollbackTransactions.Add(newInventoryTransaction);
            }
            await _unitOfWork.GetRepository<InventoryTransactions>().InsertRangeAsync(rollbackTransactions);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isSuccess)
            {
                _logger.Error("Failed to rollback inventory transactions for order {OrderId}", context.Message.OrderId);
                throw new Exception("Failed to rollback inventory transactions");
            }
            _logger.Information("Successfully rolled back inventory transactions for order {OrderId}", context.Message.OrderId);
            
        }
        else
        {
            _logger.Warning("No inventory transactions found for order {OrderId} to rollback", context.Message.OrderId);
        }
        
    }
}