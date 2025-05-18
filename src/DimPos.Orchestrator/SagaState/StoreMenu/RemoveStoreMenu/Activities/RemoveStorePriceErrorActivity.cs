using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.RemoveMenuForStore;

namespace DimPos.Orchestrator.SagaState.StoreMenu.RemoveStoreMenu.Activities;

public class RemoveStorePriceErrorActivity : IStateMachineActivity<RemoveStoreMenuSagaState, RemoveStorePriceErrorModel>
{
    private readonly ITopicProducer<Null, RollbackRemoveStoreMenuModel> _topicProducer;
    
    public RemoveStorePriceErrorActivity(ITopicProducer<Null, RollbackRemoveStoreMenuModel> topicProducer)
    {
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("RemoveStorePriceErrorActivity");
    }

    public async Task Execute(BehaviorContext<RemoveStoreMenuSagaState, RemoveStorePriceErrorModel> context, IBehavior<RemoveStoreMenuSagaState, RemoveStorePriceErrorModel> next)
    {
        var rollbackRemoveStoreMenuModel = new RollbackRemoveStoreMenuModel()
        {
            CorrelationId = context.Message.CorrelationId,
            BrandId = context.Message.BrandId,
            StoreMenuAssignments = context.Message.StoreMenuAssignments
        };
        await _topicProducer.Produce(
            key: null,
            rollbackRemoveStoreMenuModel,
            cancellationToken: context.CancellationToken
        );
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<RemoveStoreMenuSagaState, RemoveStorePriceErrorModel, TException> context, IBehavior<RemoveStoreMenuSagaState, RemoveStorePriceErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);
}