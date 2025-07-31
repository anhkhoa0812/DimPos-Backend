using DimPos.Payment.Domain.Entities;
using DimPos.Payment.Domain.Enums;
using DimPos.Payment.Infrastructure.Persistence;
using DimPos.Payment.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Payment.UpdatePaymentTransaction;

namespace DimPos.Payment.Application.Consumers;

public class RollbackPaymentTransactionConsumer : IConsumer<RollbackPaymentTransactionRequestModel>
{
    private readonly IUnitOfWork<PaymentContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public RollbackPaymentTransactionConsumer(IUnitOfWork<PaymentContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Consume(ConsumeContext<RollbackPaymentTransactionRequestModel> context)
    {
        var paymentTransaction = await _unitOfWork.GetRepository<PaymentTransactions>().SingleOrDefaultAsync(
            predicate: x => x.Id == context.Message.PaymentTransactionId
                            && x.OrderId == context.Message.OrderId
        );

        paymentTransaction.Status = EPaymentTransactionStatus.FAILED;
        
        _unitOfWork.GetRepository<PaymentTransactions>().UpdateAsync(paymentTransaction);
        var isSuccess = await _unitOfWork.CommitAsync() > 0; 
        if (!isSuccess)
        {
            _logger.Error("Failed to rollback payment transaction for OrderId: {OrderId}, PaymentTransactionId: {PaymentTransactionId}",
                context.Message.OrderId, context.Message.PaymentTransactionId);
            throw new Exception("Failed to rollback payment transaction");
        }
        _logger.Information("Successfully rolled back payment transaction for OrderId: {OrderId}, PaymentTransactionId: {PaymentTransactionId}",
            context.Message.OrderId, context.Message.PaymentTransactionId);
    }
}