using Confluent.Kafka;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Enums;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Payment.UpdatePaymentTransaction;

namespace DimPos.Order.Application.Consumers;

public class UpdateOrderStatusRequestConsumer : IConsumer<UpdateOrderStatusRequestModel>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, UpdateOrderStatusResponseModel> _successTopicProducer;
    private readonly ITopicProducer<Null, UpdateOrderStatusErrorModel> _errorTopicProducer;

    public UpdateOrderStatusRequestConsumer(IUnitOfWork<OrderContext> unitOfWork, ILogger logger,
        ITopicProducer<Null, UpdateOrderStatusResponseModel> successTopicProducer,
        ITopicProducer<Null, UpdateOrderStatusErrorModel> errorTopicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _successTopicProducer = successTopicProducer ?? throw new ArgumentNullException(nameof(successTopicProducer));
        _errorTopicProducer = errorTopicProducer ?? throw new ArgumentNullException(nameof(errorTopicProducer));
    }
    
    public async Task Consume(ConsumeContext<UpdateOrderStatusRequestModel> context)
    {
        try
        {
            var order = await _unitOfWork.GetRepository<Orders>().SingleOrDefaultAsync(
                predicate: x => x.Id == context.Message.OrderId
                                && x.Status == EOrderStatus.PendingPayment
            );
        
            order.Status = context.Message.IsPaymentSuccess
                ? EOrderStatus.Confirmed
                : EOrderStatus.Cancelled;
        
            _unitOfWork.GetRepository<Orders>().UpdateAsync(order);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isSuccess)
            {
                throw new Exception("Lỗi khi cập nhật trạng thái đơn hàng");
            }

            var updateOrderStatusResponseModel = new UpdateOrderStatusResponseModel()
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = order.Id,
                PaymentTransactionId = context.Message.PaymentTransactionId
            };
            await _successTopicProducer.Produce(
                null,
                updateOrderStatusResponseModel,
                cancellationToken: context.CancellationToken
            ).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.Error("Lỗi khi cập nhật trạng thái đơn hàng: {Message}", e.Message);
            var updateOrderStatusErrorModel = new UpdateOrderStatusErrorModel()
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                PaymentTransactionId = context.Message.PaymentTransactionId,
                IsPaymentSuccess = context.Message.IsPaymentSuccess
            };
            await _errorTopicProducer.Produce(
                null,
                updateOrderStatusErrorModel,
                cancellationToken: context.CancellationToken
            ).ConfigureAwait(false);
        }
        
    }
}