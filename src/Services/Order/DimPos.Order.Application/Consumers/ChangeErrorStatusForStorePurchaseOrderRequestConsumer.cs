using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Enums;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.UpdateInventoryForInternalOrder;

namespace DimPos.Order.Application.Consumers;

public class ChangeErrorStatusForStorePurchaseOrderRequestConsumer : IConsumer<ChangeErrorStatusForStorePurchaseOrderRequestModel>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public ChangeErrorStatusForStorePurchaseOrderRequestConsumer(IUnitOfWork<OrderContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<ChangeErrorStatusForStorePurchaseOrderRequestModel> context)
    {
        var storePurchaseOrder = await _unitOfWork.GetRepository<StorePurchaseOrders>().SingleOrDefaultAsync(
            predicate: x => x.Id == context.Message.StorePurchaseOrderId && x.StoreId == context.Message.StoreId
        );
        
        storePurchaseOrder.Status = EStorePurchaseOrderStatus.ErrorWhenUpdatingInventory;
        _unitOfWork.GetRepository<StorePurchaseOrders>().UpdateAsync(storePurchaseOrder);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (isSuccess)
        {
            _logger.Information("ChangeErrorStatusForStorePurchaseOrderRequestConsumer: Successfully changed status for StorePurchaseOrderId: {StorePurchaseOrderId} to ErrorWhenUpdatingInventory", context.Message.StorePurchaseOrderId);
        }
        else
        {
            _logger.Error("ChangeErrorStatusForStorePurchaseOrderRequestConsumer: Failed to change status for StorePurchaseOrderId: {StorePurchaseOrderId}", context.Message.StorePurchaseOrderId);
        }
    }
}