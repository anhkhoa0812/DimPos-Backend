using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.RemoveMenuForStore;

namespace DimPos.Orchestrator.SagaState.StoreMenu.RemoveStoreMenu.Activities;

public class RemoveStoreMenuActivity : IStateMachineActivity<RemoveStoreMenuSagaState, RemoveStoreMenuModel>
{
    private readonly ITopicProducer<Null, RemoveStorePriceRequestModel> _topicProducer;
    
    public RemoveStoreMenuActivity(ITopicProducer<Null, RemoveStorePriceRequestModel> topicProducer)
    {
        _topicProducer = topicProducer;
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("RemoveStoreMenuActivity");
    }

    public async Task Execute(BehaviorContext<RemoveStoreMenuSagaState, RemoveStoreMenuModel> context, IBehavior<RemoveStoreMenuSagaState, RemoveStoreMenuModel> next)
    {
        var storePrices = context.Message.StoreIds
            .SelectMany(storeId => context.Message.ProductVariantIds, (storeId, productVariantId) =>
                new StorePriceRequest()
                {
                    StoreId = storeId,
                    ProductVariantId = productVariantId
                }).ToList();
        var removeStorePriceModel = new RemoveStorePriceRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            BrandAccountId = context.Message.BrandAccountId,
            BrandId = context.Message.BrandId,
            StorePrices = storePrices,
            StoreMenuAssignments = context.Message.StoreMenuAssignments
        };
        await _topicProducer.Produce(
            key: null,
            removeStorePriceModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<RemoveStoreMenuSagaState, RemoveStoreMenuModel, TException> context, IBehavior<RemoveStoreMenuSagaState, RemoveStoreMenuModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);
}