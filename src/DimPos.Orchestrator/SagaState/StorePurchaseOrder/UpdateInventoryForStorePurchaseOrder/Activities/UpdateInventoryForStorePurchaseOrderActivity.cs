using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.UpdateInventoryForInternalOrder;

namespace DimPos.Orchestrator.SagaState.StorePurchaseOrder.UpdateInventoryForStorePurchaseOrder.Activities;

public class UpdateInventoryForStorePurchaseOrderActivity : IStateMachineActivity<UpdateInventoryForStorePurchaseOrderSagaState, GetIngredientDetailsResponseModel>
{
    private readonly ITopicProducer<Null, UpdateInventoryForInternalOrderRequestModel> _topicProducer;
    
    public UpdateInventoryForStorePurchaseOrderActivity(ITopicProducer<Null, UpdateInventoryForInternalOrderRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("UpdateInventoryForStorePurchaseOrderActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<UpdateInventoryForStorePurchaseOrderSagaState, GetIngredientDetailsResponseModel> context, IBehavior<UpdateInventoryForStorePurchaseOrderSagaState, GetIngredientDetailsResponseModel> next)
    {
        var updateInventoryRequestModel = new UpdateInventoryForInternalOrderRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            StoreId = context.Message.StoreId,
            StorePurchaseOrderId = context.Message.StorePurchaseOrderId,
            IngredientDetailsModels = context.Message.IngredientDetails
        };
        await _topicProducer.Produce(
            key: null,
            updateInventoryRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdateInventoryForStorePurchaseOrderSagaState, GetIngredientDetailsResponseModel, TException> context, IBehavior<UpdateInventoryForStorePurchaseOrderSagaState, GetIngredientDetailsResponseModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}