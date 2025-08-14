using Confluent.Kafka;
using DimPos.Payment.Domain.Entities;
using DimPos.Payment.Domain.Enums;
using DimPos.Payment.Infrastructure.Persistence;
using DimPos.Payment.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Payment.UpdatePaymentTransaction;

namespace DimPos.Payment.Application.Consumers;

public class UpdatePaymentTransactionRequestConsumer : IConsumer<UpdatePaymentTransactionRequestModel>
{
    private readonly IUnitOfWork<PaymentContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, UpdatePaymentTransactionResponseModel> _topicProducer;
    public UpdatePaymentTransactionRequestConsumer(IUnitOfWork<PaymentContext> unitOfWork, ILogger logger,
        ITopicProducer<Null, UpdatePaymentTransactionResponseModel> topicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public async Task Consume(ConsumeContext<UpdatePaymentTransactionRequestModel> context)
    {
        var paymentTransaction = await _unitOfWork.GetRepository<PaymentTransactions>().SingleOrDefaultAsync(
            predicate: x => x.OrderId == context.Message.OrderId
        );

        if (context.Message.Type == PaymentCallbackType.MPos)
        {
            var status = context.Message.TransStatus;
            switch (status)
            {
                case MPosTransStatus.Settled:
                    paymentTransaction.Status = EPaymentTransactionStatus.SUCCESS;
                    break;
                case MPosTransStatus.Fail:
                case MPosTransStatus.Rejected:
                case MPosTransStatus.Voided:
                    paymentTransaction.Status = EPaymentTransactionStatus.FAILED;
                    break;
            }
        }
        else if (context.Message.Type == PaymentCallbackType.PayOs )
        {
            paymentTransaction.Status = EPaymentTransactionStatus.SUCCESS;
        }

        _unitOfWork.GetRepository<PaymentTransactions>().UpdateAsync(paymentTransaction);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (isSuccess)
        {
            var updatePaymentTransactionResponse = new UpdatePaymentTransactionResponseModel()
            {
                CorrelationId = context.Message.CorrelationId,
                TransStatus = context.Message.TransStatus,
                OrderId = context.Message.OrderId,
                PaymentTransactionId = paymentTransaction.Id,
                Type = context.Message.Type
            };
            await _topicProducer.Produce(
                null,
                updatePaymentTransactionResponse,
                cancellationToken: context.CancellationToken
            ).ConfigureAwait(false);
        }
    }
}