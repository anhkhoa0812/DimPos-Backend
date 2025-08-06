using DimPos.Order.Domain.Entities;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Order.ChangeIsNeedToUpdateInventoryForOrder;

namespace DimPos.Order.Application.Consumers;

public class ChangeIsNeedToUpdateInventoryForOrderConsumer : IConsumer<ChangeIsNeedToUpdateInventoryForOrderRequestModel>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public ChangeIsNeedToUpdateInventoryForOrderConsumer(IUnitOfWork<OrderContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<ChangeIsNeedToUpdateInventoryForOrderRequestModel> context)
    {
        var order = await _unitOfWork.GetRepository<Orders>().SingleOrDefaultAsync(
            predicate: x => x.Id == context.Message.OrderId
        );
        if (order == null)
        {
            _logger.Error("Order with ID {OrderId} not found", context.Message.OrderId);
            throw new BadHttpRequestException($"Order with ID {context.Message.OrderId} not found");
        }
        
        order.IsNeedToUpdateInventory = false;
        _unitOfWork.GetRepository<Orders>().UpdateAsync(order);
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Failed to update order {OrderId} to need inventory update", context.Message.OrderId);
            throw new Exception($"Failed to update order {context.Message.OrderId} to need inventory update");
        }
        _logger.Information("Order {OrderId} updated to need inventory update successfully", context.Message.OrderId);
    }
}