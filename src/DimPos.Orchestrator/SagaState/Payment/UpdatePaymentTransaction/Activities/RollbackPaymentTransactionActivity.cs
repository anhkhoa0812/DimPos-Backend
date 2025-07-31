using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Payment.UpdatePaymentTransaction;

namespace DimPos.Orchestrator.SagaState.Payment.UpdatePaymentTransaction.Activities;

public class RollbackPaymentTransactionActivity : IStateMachineActivity<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel>
{
    private readonly ITopicProducer<Null, RollbackPaymentTransactionRequestModel> _topicProducer;

    public RollbackPaymentTransactionActivity(ITopicProducer<Null, RollbackPaymentTransactionRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("RollbackPaymentTransactionActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);


    public async Task Execute(BehaviorContext<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel> context, IBehavior<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel> next)
    {
        var rollbackPaymentTransactionRequestModel = new RollbackPaymentTransactionRequestModel()
        {
            CorrelationId = context.Saga.CorrelationId,
            OrderId = context.Message.OrderId,
            PaymentTransactionId = context.Message.PaymentTransactionId,
            IsPaymentSuccess = context.Message.IsPaymentSuccess
        };
        await _topicProducer.Produce(
            null,
            rollbackPaymentTransactionRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
        
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel, TException> context, IBehavior<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);

    }
}