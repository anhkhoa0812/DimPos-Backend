using Confluent.Kafka;
using DimPos.Payment.Domain.Entities;
using DimPos.Payment.Domain.Enums;
using DimPos.Payment.Infrastructure.Persistence;
using DimPos.Payment.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Order.UpdatePaymentTransactionForCashOrder;

namespace DimPos.Payment.Application.Consumers;

public class UpdatePaymentTransactionForCashOrderConsumer : IConsumer<UpdatePaymentTransactionForCashOrderRequestModel>
{
    private readonly IUnitOfWork<PaymentContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, UpdatePaymentTransactionForCashOrderResponseModel> _successTopicProducer;
    private readonly ITopicProducer<Null, UpdatePaymentTransactionForCashOrderErrorModel> _errorTopicProducer;
    
    public UpdatePaymentTransactionForCashOrderConsumer(IUnitOfWork<PaymentContext> unitOfWork, ILogger logger,
        ITopicProducer<Null, UpdatePaymentTransactionForCashOrderResponseModel> successTopicProducer,
        ITopicProducer<Null, UpdatePaymentTransactionForCashOrderErrorModel> errorTopicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _successTopicProducer = successTopicProducer ?? throw new ArgumentNullException(nameof(successTopicProducer));
        _errorTopicProducer = errorTopicProducer ?? throw new ArgumentNullException(nameof(errorTopicProducer));
    }
    
    public async Task Consume(ConsumeContext<UpdatePaymentTransactionForCashOrderRequestModel> context)
    {
        try
        {
            var paymentTransaction = await _unitOfWork.GetRepository<Domain.Entities.PaymentTransactions>().SingleOrDefaultAsync(
                predicate: x => x.OrderId == context.Message.OrderId && x.StoreId == context.Message.StoreId
                                                                     && x.Id == context.Message.PaymentTransactionId
                                                                     && x.Status == EPaymentTransactionStatus.PENDING
            );
            if (paymentTransaction == null)
            {
                throw new BadHttpRequestException("Không tìm thấy giao dịch thanh toán hoặc giao dịch không ở trạng thái chờ xử lý");
            }
            paymentTransaction.Status = EPaymentTransactionStatus.SUCCESS;
            _unitOfWork.GetRepository<PaymentTransactions>().UpdateAsync(paymentTransaction);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isSuccess)
            {
                _logger.Error("Cập nhật giao dịch thanh toán cho đơn hàng {OrderId} không thành công", context.Message.OrderId);
                throw new Exception("Cập nhật giao dịch thanh toán không thành công");
            }
            var updatePaymentTransactionForCashOrderResponseModel = new UpdatePaymentTransactionForCashOrderResponseModel()
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
            };
            await _successTopicProducer.Produce(
                null,
                updatePaymentTransactionForCashOrderResponseModel,
                cancellationToken: context.CancellationToken
            );
            _logger.Information("Cập nhật giao dịch thanh toán cho đơn hàng {OrderId} thành công", context.Message.OrderId);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Lỗi khi cập nhật giao dịch thanh toán cho đơn hàng {OrderId}", context.Message.OrderId);
            var updatePaymentTransactionForCashOrderErrorModel = new UpdatePaymentTransactionForCashOrderErrorModel()
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                ErrorMessage = e.Message
            };
            await _errorTopicProducer.Produce(
                null,
                updatePaymentTransactionForCashOrderErrorModel,
                cancellationToken: context.CancellationToken
            );
        }
    }
}