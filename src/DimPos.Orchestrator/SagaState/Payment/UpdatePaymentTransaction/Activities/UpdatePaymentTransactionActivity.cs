using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Payment.UpdatePaymentTransaction;

namespace DimPos.Orchestrator.SagaState.Payment.UpdatePaymentTransaction.Activities;

public class UpdatePaymentTransactionActivity : IStateMachineActivity<UpdatePaymentTransactionSagaState, CallbackPaymentResponseModel>
{
    private readonly ITopicProducer<Null, UpdatePaymentTransactionRequestModel> _topicProducer;
    
    public UpdatePaymentTransactionActivity(ITopicProducer<Null, UpdatePaymentTransactionRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("UpdatePaymentTransactionActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<UpdatePaymentTransactionSagaState, CallbackPaymentResponseModel> context, IBehavior<UpdatePaymentTransactionSagaState, CallbackPaymentResponseModel> next)
    {
        var updatePaymentTransactionRequestModel = new UpdatePaymentTransactionRequestModel()
        {
            CorrelationId = context.Saga.CorrelationId,
            TransStatus = context.Message.TransStatus,
            TransCode = context.Message.TransCode,
            TransAmount = context.Message.TransAmount,
            OrderId = context.Message.OrderId,
            Type = context.Message.Type
        };
        await _topicProducer.Produce(
            null,
            updatePaymentTransactionRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdatePaymentTransactionSagaState, CallbackPaymentResponseModel, TException> context, IBehavior<UpdatePaymentTransactionSagaState, CallbackPaymentResponseModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}