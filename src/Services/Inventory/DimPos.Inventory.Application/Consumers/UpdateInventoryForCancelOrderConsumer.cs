using Confluent.Kafka;
using DimPos.Inventory.Domain.Entities;
using DimPos.Inventory.Domain.Enums;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.Inventory.Infrastructure.Repositories.Interface;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedProject.Events.Order.CancelOrder;

namespace DimPos.Inventory.Application.Consumers;

public class UpdateInventoryForCancelOrderConsumer : IConsumer<UpdateInventoryForCancelOrderRequestModel>
{
    private readonly IUnitOfWork<InventoryContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, UpdateInventoryForCancelOrderResponseModel> _successTopicProducer;
    private readonly ITopicProducer<Null, UpdateInventoryForCancelOrderErrorModel> _errorTopicProducer;
    
    public UpdateInventoryForCancelOrderConsumer(IUnitOfWork<InventoryContext> unitOfWork, ILogger logger,
        ITopicProducer<Null, UpdateInventoryForCancelOrderResponseModel> successTopicProducer,
        ITopicProducer<Null, UpdateInventoryForCancelOrderErrorModel> errorTopicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _successTopicProducer = successTopicProducer ?? throw new ArgumentNullException(nameof(successTopicProducer));
        _errorTopicProducer = errorTopicProducer ?? throw new ArgumentNullException(nameof(errorTopicProducer));
    }
    
    public async Task Consume(ConsumeContext<UpdateInventoryForCancelOrderRequestModel> context)
    {
        try
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
                    throw new Exception($"Không thể hoàn tác giao dịch kho hàng trong quá trình hủy đơn hàng: {context.Message.OrderId}");
                }
                _logger.Information("Successfully rolled back inventory transactions for order {OrderId}", context.Message.OrderId);
            }
            else
            {
                _logger.Warning("No inventory transactions found for order {OrderId} to rollback", context.Message.OrderId);
            }

            var updateInventoryForCancelOrderResponseModel = new UpdateInventoryForCancelOrderResponseModel()
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId
            };
            await _successTopicProducer.Produce(
                null,
                updateInventoryForCancelOrderResponseModel,
                cancellationToken: context.CancellationToken
            ).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            var updateInventoryForCancelOrderErrorModel = new UpdateInventoryForCancelOrderErrorModel()
            {
                CorrelationId = context.Message.CorrelationId,
                AccountId = context.Message.AccountId,
                OrderId = context.Message.OrderId,
                ErrorMessage = e.Message,
                StoreId = context.Message.StoreId
            };
            await _errorTopicProducer.Produce(
                null,
                updateInventoryForCancelOrderErrorModel,
                cancellationToken: context.CancellationToken
            ).ConfigureAwait(false);
        }
    }
}