using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Payment.UpdatePaymentTransaction;

namespace DimPos.Orchestrator.SagaState.Payment.UpdatePaymentTransaction.Activities;

public class RollbackInventoryForOrderActivity : IStateMachineActivity<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel>
{
    private readonly ITopicProducer<Null, RollbackInventoryForOrderRequestModel> _topicProducer;

    public RollbackInventoryForOrderActivity(ITopicProducer<Null, RollbackInventoryForOrderRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    } 
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("RollbackInventoryForOrderActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);


    public async Task Execute(BehaviorContext<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel> context, IBehavior<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel> next)
    {
        if (context.Message.IsPaymentSuccess)
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
            await next.Execute(context).ConfigureAwait(false);
        }
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel, TException> context, IBehavior<UpdatePaymentTransactionSagaState, UpdateOrderStatusErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}