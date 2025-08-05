using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.AssignMenuForStore;

namespace DimPos.Orchestrator.SagaState.StoreMenu.AssignNewStoreMenu.Activities;

public class AssignNewStoreMenuActivity : IStateMachineActivity<AssignNewStoreMenuSagaState, AssignNewStoreMenuModel>
{
    private readonly ITopicProducer<Null, AddStorePriceRequestModel> _topicProducer;
    public AssignNewStoreMenuActivity(ITopicProducer<Null, AddStorePriceRequestModel> topicProducer)
    {
        _topicProducer = topicProducer;
    }
    public void Probe(ProbeContext context)
    {
        context.CreateScope("AssignNewStoreMenuActivity");
    }
    

    public async Task Execute(BehaviorContext<AssignNewStoreMenuSagaState, AssignNewStoreMenuModel> context, IBehavior<AssignNewStoreMenuSagaState, AssignNewStoreMenuModel> next)
    {
        var storePrices = context.Message.StoreIds
            .SelectMany(storeId => context.Message.ProductVariantIds, (storeId, productVariantId) =>
                new StorePriceRequest()
                {
                    StoreId = storeId,
                    ProductVariantId = productVariantId
                }).ToList();
        var addStorePriceModel = new AddStorePriceRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            BrandAccountId = context.Message.BrandAccountId,
            BrandId = context.Message.BrandId,
            StorePrices = storePrices,
            BrandMenuId = context.Message.BrandMenuId
        };
        await _topicProducer.Produce(
            key: null,
            addStorePriceModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<AssignNewStoreMenuSagaState, AssignNewStoreMenuModel, TException> context, IBehavior<AssignNewStoreMenuSagaState, AssignNewStoreMenuModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);
}