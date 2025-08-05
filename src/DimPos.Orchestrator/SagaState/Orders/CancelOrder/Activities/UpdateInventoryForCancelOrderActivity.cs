using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Order.CancelOrder;

namespace DimPos.Orchestrator.SagaState.Orders.CancelOrder.Activities;

public class UpdateInventoryForCancelOrderActivity : IStateMachineActivity<CancelOrderSagaState, CancelOrderResponseModel>
{
    private readonly ITopicProducer<Null, UpdateInventoryForCancelOrderRequestModel> _topicProducer;
    
    public UpdateInventoryForCancelOrderActivity(ITopicProducer<Null, UpdateInventoryForCancelOrderRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("UpdateInventoryForCancelOrderActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);


    public async Task Execute(BehaviorContext<CancelOrderSagaState, CancelOrderResponseModel> context, IBehavior<CancelOrderSagaState, CancelOrderResponseModel> next)
    {
        var updateInventoryForCancelOrderRequestModel = new UpdateInventoryForCancelOrderRequestModel
        {
            CorrelationId = context.Message.CorrelationId,
            OrderId = context.Message.OrderId,
            StoreId = context.Message.StoreId,
            AccountId = context.Message.AccountId
        };

        await _topicProducer.Produce(
            null,
            updateInventoryForCancelOrderRequestModel,
            context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<CancelOrderSagaState, CancelOrderResponseModel, TException> context, IBehavior<CancelOrderSagaState, CancelOrderResponseModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);    
    }
}