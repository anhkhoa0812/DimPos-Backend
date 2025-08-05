using Confluent.Kafka;
using DimPos.Inventory.Domain.Entities;
using DimPos.Inventory.Domain.Enums;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.Inventory.Infrastructure.Repositories.Interface;
using DimPos.Inventory.Infrastructure.Utils;
using MassTransit;
using SharedProject.Events.UpdateInventoryForInternalOrder;

namespace DimPos.Inventory.Application.Consumers;

public class UpdateInventoryForInternalOrderRequestConsumer : IConsumer<UpdateInventoryForInternalOrderRequestModel>
{
    private readonly IUnitOfWork<InventoryContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, UpdateInventoryForInternalOrderResponseModel> _successTopicProducer;
    private readonly ITopicProducer<Null, UpdateInventoryForInternalOrderErrorModel> _errorTopicProducer;
    
    public UpdateInventoryForInternalOrderRequestConsumer(IUnitOfWork<InventoryContext> unitOfWork, ILogger logger,
        ITopicProducer<Null, UpdateInventoryForInternalOrderResponseModel> successTopicProducer,
        ITopicProducer<Null, UpdateInventoryForInternalOrderErrorModel> errorTopicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _successTopicProducer = successTopicProducer ?? throw new ArgumentNullException(nameof(successTopicProducer));
        _errorTopicProducer = errorTopicProducer ?? throw new ArgumentNullException(nameof(errorTopicProducer));
    }
    
    public async Task Consume(ConsumeContext<UpdateInventoryForInternalOrderRequestModel> context)
    {
        try
        {
            foreach (var ingredientDetailsModel in context.Message.IngredientDetailsModels) 
            {
                var inventoryStock = await _unitOfWork.GetRepository<InventoryStock>().SingleOrDefaultAsync(
                    predicate: x => x.StoreId == context.Message.StoreId && x.IngredientId == ingredientDetailsModel.IngredientId
                );
                if (inventoryStock == null)
                {
                    var newInventoryStock = new InventoryStock()
                    {
                        Id = Guid.CreateVersion7(),
                        StoreId = context.Message.StoreId,
                        IngredientId = ingredientDetailsModel.IngredientId,
                        ReOrderLevel = 0,
                        Quantity = ingredientDetailsModel.Quantity,
                        InventoryTransactions = new List<InventoryTransactions>()
                        {
                            new InventoryTransactions()
                            {
                                Id = Guid.CreateVersion7(),
                                Note = "",
                                Type = EInventoryTransactionType.ReceiptFromInternalPo,
                                QuantityChange = ingredientDetailsModel.Quantity,
                                RelatedStorePurchaseOrderItemId = context.Message.StorePurchaseOrderId,
                            }
                        }
                    };
                    await _unitOfWork.GetRepository<InventoryStock>().InsertAsync(newInventoryStock);
                }
                else
                {
                    inventoryStock.Quantity += ingredientDetailsModel.Quantity;
                    inventoryStock.InventoryTransactions.Add(new InventoryTransactions()
                    {
                        Id = Guid.CreateVersion7(),
                        Note = "",
                        Type = EInventoryTransactionType.ReceiptFromInternalPo,
                        QuantityChange = ingredientDetailsModel.Quantity,
                        RelatedStorePurchaseOrderItemId = context.Message.StorePurchaseOrderId,
                    });
                    _unitOfWork.GetRepository<InventoryStock>().UpdateAsync(inventoryStock);
                }
            }
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isSuccess)
            {
                throw new Exception($"Lỗi khi cập nhật kho cho đơn hàng nội bộ {context.Message.StorePurchaseOrderId}");
            }
            _logger.Information("Successfully updated inventory for internal order: {MessageCorrelationId}", context.Message.CorrelationId);
            var updateInventoryForInternalOrderResponseModel = new UpdateInventoryForInternalOrderResponseModel()
            {
                CorrelationId = context.Message.CorrelationId,
                StoreId = context.Message.StoreId,
                StorePurchaseOrderId = context.Message.StorePurchaseOrderId
            };
            await _successTopicProducer.Produce(
                key: null,
                updateInventoryForInternalOrderResponseModel,
                cancellationToken: context.CancellationToken
            );
        }
        catch (Exception e)
        {
            var updateInventoryForInternalOrderErrorModel = new UpdateInventoryForInternalOrderErrorModel()
            {
                CorrelationId = context.Message.CorrelationId,
                AccountId = context.Message.AccountId,
                StoreId = context.Message.StoreId,
                StorePurchaseOrderId = context.Message.StorePurchaseOrderId,
                Message = e.Message
            };
            _logger.Error(e, $"Error updating inventory for internal order: {context.Message.CorrelationId}");
            await _errorTopicProducer.Produce(
                key: null,
                updateInventoryForInternalOrderErrorModel,
                cancellationToken: context.CancellationToken
            );
        }
    }
}