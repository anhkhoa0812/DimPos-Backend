using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.UpdateInventoryForInternalOrder;

namespace DimPos.Orchestrator.SagaState.StorePurchaseOrder.UpdateInventoryForStorePurchaseOrder.Activities;

public class GetIngredientDetailsActivity : IStateMachineActivity<UpdateInventoryForStorePurchaseOrderSagaState, InternalOrderDoneByStoreResponseModel>
{
    private readonly ITopicProducer<Null, GetIngredientDetailsRequestModel> _topicProducer;
    
    public GetIngredientDetailsActivity(ITopicProducer<Null, GetIngredientDetailsRequestModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("GetIngredientDetailsActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);


    public async Task Execute(BehaviorContext<UpdateInventoryForStorePurchaseOrderSagaState, InternalOrderDoneByStoreResponseModel> context, IBehavior<UpdateInventoryForStorePurchaseOrderSagaState, InternalOrderDoneByStoreResponseModel> next)
    {
        var getIngredientDetailsRequestModel = new GetIngredientDetailsRequestModel
        {
            CorrelationId = context.Message.CorrelationId,
            AccountId = context.Message.AccountId,
            StoreId = context.Message.StoreId,
            StorePurchaseOrderId = context.Message.StorePurchaseOrderId,
            StorePurchaseOrderItems = context.Message.StorePurchaseOrderItems,
        };
        await _topicProducer.Produce(
            key: null,
            getIngredientDetailsRequestModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdateInventoryForStorePurchaseOrderSagaState, InternalOrderDoneByStoreResponseModel, TException> context, IBehavior<UpdateInventoryForStorePurchaseOrderSagaState, InternalOrderDoneByStoreResponseModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}