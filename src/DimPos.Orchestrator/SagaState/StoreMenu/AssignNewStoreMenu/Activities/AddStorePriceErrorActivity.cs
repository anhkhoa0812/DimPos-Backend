using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.AssignMenuForStore;

namespace DimPos.Orchestrator.SagaState.StoreMenu.AssignNewStoreMenu.Activities;

public class AddStorePriceErrorActivity : IStateMachineActivity<AssignNewStoreMenuSagaState, AddStorePriceErrorModel>
{
    private readonly ITopicProducer<Null, RollbackStoreMenuModel> _topicProducer;
    public AddStorePriceErrorActivity(ITopicProducer<Null, RollbackStoreMenuModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));   
    }
    public void Probe(ProbeContext context)
    {
        context.CreateScope("AddStorePriceErrorActivity");
    }
    

    public async Task Execute(BehaviorContext<AssignNewStoreMenuSagaState, AddStorePriceErrorModel> context, IBehavior<AssignNewStoreMenuSagaState, AddStorePriceErrorModel> next)
    {
        var rollbackStoreMenuModel = new RollbackStoreMenuModel()
        {
            CorrelationId = context.Message.CorrelationId,
            BrandId = context.Message.BrandId,
            StoreIds = context.Message.StoreIds,
            BrandMenuId = context.Message.BrandMenuId
        };
        await _topicProducer.Produce(
            key: null,
            rollbackStoreMenuModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<AssignNewStoreMenuSagaState, AddStorePriceErrorModel, TException> context, IBehavior<AssignNewStoreMenuSagaState, AddStorePriceErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);
}