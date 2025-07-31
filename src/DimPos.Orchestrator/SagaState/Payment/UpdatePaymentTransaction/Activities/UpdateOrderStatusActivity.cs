using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Payment.UpdatePaymentTransaction;

namespace DimPos.Orchestrator.SagaState.Payment.UpdatePaymentTransaction.Activities;

public class UpdateOrderStatusActivity :  IStateMachineActivity<UpdatePaymentTransactionSagaState, UpdatePaymentTransactionResponseModel>
{
    private readonly ITopicProducer<Null, UpdateOrderStatusRequestModel> _topicProducer;

    public UpdateOrderStatusActivity(ITopicProducer<Null, UpdateOrderStatusRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("UpdateOrderStatusActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<UpdatePaymentTransactionSagaState, UpdatePaymentTransactionResponseModel> context, IBehavior<UpdatePaymentTransactionSagaState, UpdatePaymentTransactionResponseModel> next)
    {
        var updateOrderStatusRequestModel = new UpdateOrderStatusRequestModel()
        {
            CorrelationId = context.Saga.CorrelationId,
            OrderId = context.Message.OrderId,
            PaymentTransactionId = context.Message.PaymentTransactionId,
            IsPaymentSuccess = context.Message.TransStatus == MPosTransStatus.Settled
        };
        await _topicProducer.Produce(
            null,
            updateOrderStatusRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdatePaymentTransactionSagaState, UpdatePaymentTransactionResponseModel, TException> context, IBehavior<UpdatePaymentTransactionSagaState, UpdatePaymentTransactionResponseModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}