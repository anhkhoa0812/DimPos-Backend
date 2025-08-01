using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Payment.UpdatePaymentTransaction;

namespace DimPos.Orchestrator.SagaState.Payment.UpdatePaymentTransaction.Activities;

public class RollbackInventoryForFailedPaymentActivity : IStateMachineActivity<UpdatePaymentTransactionSagaState, UpdatePaymentTransactionResponseModel>
{
    private readonly ITopicProducer<Null, RollbackInventoryForOrderRequestModel> _topicProducer;

    public RollbackInventoryForFailedPaymentActivity(ITopicProducer<Null, RollbackInventoryForOrderRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    } 

    public void Probe(ProbeContext context)
    {
        context.CreateScope("RollbackInventoryForFailedPaymentActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);


    public async Task Execute(BehaviorContext<UpdatePaymentTransactionSagaState, UpdatePaymentTransactionResponseModel> context, IBehavior<UpdatePaymentTransactionSagaState, UpdatePaymentTransactionResponseModel> next)
    {
        if (context.Message.TransStatus != MPosTransStatus.Settled)
        {
            var rollbackInventoryForOrderRequestModel = new RollbackInventoryForOrderRequestModel()
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
            };
            await _topicProducer.Produce(
                null,
                rollbackInventoryForOrderRequestModel,
                cancellationToken: context.CancellationToken
            );
        }
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdatePaymentTransactionSagaState, UpdatePaymentTransactionResponseModel, TException> context, IBehavior<UpdatePaymentTransactionSagaState, UpdatePaymentTransactionResponseModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}