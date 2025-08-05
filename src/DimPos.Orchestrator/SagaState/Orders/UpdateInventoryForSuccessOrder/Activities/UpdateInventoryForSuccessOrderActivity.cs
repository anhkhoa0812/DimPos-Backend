using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Order.UpdateInventoryForSuccessOrder;

namespace DimPos.Orchestrator.SagaState.Orders.UpdateInventoryForSuccessOrder.Activities;

public class UpdateInventoryForSuccessOrderActivity : IStateMachineActivity<UpdateInventoryForSuccessOrderSagaState, CreateOrderResponseModel>
{
    private readonly ITopicProducer<Null, UpdateInventoryForSuccessOrderRequestModel> _topicProducer;

    public UpdateInventoryForSuccessOrderActivity(
        ITopicProducer<Null, UpdateInventoryForSuccessOrderRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    public void Probe(ProbeContext context)
    {
        context.CreateScope("UpdateInventoryForSuccessOrderActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<UpdateInventoryForSuccessOrderSagaState, CreateOrderResponseModel> context, IBehavior<UpdateInventoryForSuccessOrderSagaState, CreateOrderResponseModel> next)
    {
        var updateInventoryForSuccessOrderRequestModel = new UpdateInventoryForSuccessOrderRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            AccountId = context.Message.AccountId,
            OrderId = context.Message.OrderId,
            StoreId = context.Message.StoreId,
            Ingredients = context.Message.Ingredients
        };
        await _topicProducer.Produce(
            null,
            updateInventoryForSuccessOrderRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdateInventoryForSuccessOrderSagaState, CreateOrderResponseModel, TException> context, IBehavior<UpdateInventoryForSuccessOrderSagaState, CreateOrderResponseModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}