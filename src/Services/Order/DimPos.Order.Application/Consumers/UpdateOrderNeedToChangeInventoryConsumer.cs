using DimPos.Order.Domain.Entities;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Order.UpdateInventoryForSuccessOrder;

namespace DimPos.Order.Application.Consumers;

public class UpdateOrderNeedToChangeInventoryConsumer : IConsumer<UpdateOrderNeedToChangeInventoryRequestModel>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public UpdateOrderNeedToChangeInventoryConsumer(IUnitOfWork<OrderContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<UpdateOrderNeedToChangeInventoryRequestModel> context)
    {
        var order = await _unitOfWork.GetRepository<Orders>().SingleOrDefaultAsync(
            predicate: x => x.Id == context.Message.OrderId
            && x.StoreId == context.Message.StoreId
        );
        order.IsNeedToUpdateInventory = true;
        
        _unitOfWork.GetRepository<Orders>().UpdateAsync(order);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Failed to update order {OrderId} to need inventory change at store {StoreId}",
                context.Message.OrderId, context.Message.StoreId);
        }
        _logger.Information("Order {OrderId} at store {StoreId} marked as needing inventory change",
            context.Message.OrderId, context.Message.StoreId);
    }
}