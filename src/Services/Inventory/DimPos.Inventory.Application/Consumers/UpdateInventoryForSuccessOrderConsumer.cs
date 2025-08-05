using Confluent.Kafka;
using DimPos.Inventory.Domain.Entities;
using DimPos.Inventory.Domain.Enums;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.Inventory.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Order.UpdateInventoryForSuccessOrder;

namespace DimPos.Inventory.Application.Consumers;

public class UpdateInventoryForSuccessOrderConsumer : IConsumer<UpdateInventoryForSuccessOrderRequestModel>
{
    private readonly IUnitOfWork<InventoryContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, UpdateInventoryForSuccessOrderResponseModel> _successTopicProducer;
    private readonly ITopicProducer<Null, UpdateInventoryForSuccessOrderErrorModel> _errorTopicProducer;
    
    public UpdateInventoryForSuccessOrderConsumer(IUnitOfWork<InventoryContext> unitOfWork, ILogger logger,
        ITopicProducer<Null, UpdateInventoryForSuccessOrderResponseModel> successTopicProducer,
        ITopicProducer<Null, UpdateInventoryForSuccessOrderErrorModel> errorTopicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _successTopicProducer = successTopicProducer ?? throw new ArgumentNullException(nameof(successTopicProducer));
        _errorTopicProducer = errorTopicProducer ?? throw new ArgumentNullException(nameof(errorTopicProducer));
    }
    
    public async Task Consume(ConsumeContext<UpdateInventoryForSuccessOrderRequestModel> context)
    {
        try
        {
            var requestIngredientIds = context.Message.Ingredients.Select(x => x.IngredientId).ToList();

            var inventoryStocks = await _unitOfWork.GetRepository<InventoryStock>().GetListAsync(
                predicate: x => x.StoreId == context.Message.StoreId && 
                                requestIngredientIds.Contains(x.IngredientId) 
            );
        
            if(inventoryStocks.Count != requestIngredientIds.Count)
            {
                _logger.Error("Not all ingredients found in inventory for order {OrderId} at store {StoreId} with {StockCount} stocks and {RequestCount} requests", 
                    context.Message.OrderId, context.Message.StoreId,inventoryStocks.Count, requestIngredientIds.Count );
                throw new BadHttpRequestException("Lỗi cập nhật kho hàng: Không đủ nguyên liệu trong kho.");
            }
            foreach (var inventoryStock in inventoryStocks)
            {
                var requestIngredient = context.Message.Ingredients
                    .FirstOrDefault(x => x.IngredientId == inventoryStock.IngredientId);

                if (requestIngredient != null)
                {
                    if(inventoryStock.Quantity < requestIngredient.Quantity)
                    {
                        _logger.Error("Not enough stock for ingredient {IngredientId} in order {OrderId} at store {StoreId} and quantity {Quantity}",
                            requestIngredient.IngredientId, context.Message.OrderId, context.Message.StoreId, requestIngredient.Quantity);
                        throw new BadHttpRequestException($"Lỗi cập nhật kho hàng: Nguyên liệu {requestIngredient.IngredientId} không đủ trong kho cho đơn hàng {context.Message.OrderId}.");
                    }
                    
                    inventoryStock.Quantity -= requestIngredient.Quantity;
                    var newInventoryTransaction = new InventoryTransactions()
                    {
                        Id = Guid.CreateVersion7(),
                        Type = EInventoryTransactionType.ConsumptionSale,
                        QuantityChange = -requestIngredient.Quantity,
                        RelatedOrderId = context.Message.OrderId,
                        InventoryStockId = inventoryStock.Id
                    };
                    await _unitOfWork.GetRepository<InventoryTransactions>().InsertAsync(newInventoryTransaction);
                    _unitOfWork.GetRepository<InventoryStock>().UpdateAsync(inventoryStock);
                }
            }

            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isSuccess)
            {
                _logger.Error("Failed to update inventory for order {OrderId} at store {StoreId}", 
                    context.Message.OrderId, context.Message.StoreId);
                throw new Exception(
                    $"Lỗi cập nhật kho hàng: Không thể cập nhật kho. Vui lòng cập nhật kho hàng thủ công cho đơn hàng {context.Message.OrderId} sau khi đặt hàng thành công");
            }
            _logger.Information("Successfully updated inventory for order {OrderId} at store {StoreId}", 
                context.Message.OrderId, context.Message.StoreId);
            await _successTopicProducer.Produce(
                null,
                new UpdateInventoryForSuccessOrderResponseModel
                {
                    CorrelationId = context.Message.CorrelationId,
                    OrderId = context.Message.OrderId,
                    StoreId = context.Message.StoreId,
                },
                cancellationToken: context.CancellationToken
            ).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            var errorModel = new UpdateInventoryForSuccessOrderErrorModel
            {
                CorrelationId = context.Message.CorrelationId,
                AccountId = context.Message.AccountId,
                OrderId = context.Message.OrderId,
                StoreId = context.Message.StoreId,
                ErrorMessage = e.Message
            };
            _logger.Error(e, "Error updating inventory for order {OrderId} at store {StoreId}: {ErrorMessage}", 
                context.Message.OrderId, context.Message.StoreId, errorModel.ErrorMessage);
            await _errorTopicProducer.Produce(
                null,
                errorModel,
                cancellationToken: context.CancellationToken
            ).ConfigureAwait(false);
        }
    }
}