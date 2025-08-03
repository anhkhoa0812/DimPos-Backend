using DimPos.Order.Domain.Enums;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Order.UpdatePaymentTransactionForCashOrder;

namespace DimPos.Order.Application.Consumers;

public class RollbackPendingForCashOrderConsumer : IConsumer<RollbackPendingForCashOrderRequestModel>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public RollbackPendingForCashOrderConsumer(IUnitOfWork<OrderContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<RollbackPendingForCashOrderRequestModel> context)
    {
        var order = await _unitOfWork.GetRepository<Domain.Entities.Orders>().SingleOrDefaultAsync(
            predicate: x => x.Id == context.Message.OrderId
        );

        order.Status = EOrderStatus.PendingPayment;
        order.AmountPaid = 0;
        order.CashRoundingAmount = 0;
        _unitOfWork.GetRepository<Domain.Entities.Orders>().UpdateAsync(order);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            _logger.Error("Không thể hoàn tác trạng thái đơn hàng {OrderId} về trạng thái chờ thanh toán", context.Message.OrderId);
            throw new Exception("Không thể hoàn tác trạng thái đơn hàng");
        }
        _logger.Information("Đã hoàn tác trạng thái đơn hàng {OrderId} về trạng thái chờ thanh toán", context.Message.OrderId);

    }
}