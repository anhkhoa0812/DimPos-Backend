using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Order.UpdateInventoryForSuccessOrder;

namespace DimPos.Orchestrator.SagaState.Orders.UpdateInventoryForSuccessOrder.Activities;

public class UpdateOrderNeedToChangeInventoryActivity : IStateMachineActivity<UpdateInventoryForSuccessOrderSagaState, UpdateInventoryForSuccessOrderErrorModel>
{
    private readonly ITopicProducer<Null, UpdateOrderNeedToChangeInventoryRequestModel> _topicProducer;
    
    public UpdateOrderNeedToChangeInventoryActivity(
        ITopicProducer<Null, UpdateOrderNeedToChangeInventoryRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("UpdateInventoryForSuccessOrderSagaState");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);


    public async Task Execute(BehaviorContext<UpdateInventoryForSuccessOrderSagaState, UpdateInventoryForSuccessOrderErrorModel> context, IBehavior<UpdateInventoryForSuccessOrderSagaState, UpdateInventoryForSuccessOrderErrorModel> next)
    {
        var updateOrderNeedToChangeInventoryRequestModel = new UpdateOrderNeedToChangeInventoryRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            OrderId = context.Message.OrderId,
            StoreId = context.Message.StoreId,
        };
        await _topicProducer.Produce(
            null,
            updateOrderNeedToChangeInventoryRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdateInventoryForSuccessOrderSagaState, UpdateInventoryForSuccessOrderErrorModel, TException> context, IBehavior<UpdateInventoryForSuccessOrderSagaState, UpdateInventoryForSuccessOrderErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}