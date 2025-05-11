using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Brand;
using SharedProject.Events.Store.CreateStore;

namespace DimPos.Orchestrator.SagaState.Brands.CreateStoreSaga.Activities;

public class RollbackBrandActivity : IStateMachineActivity<CreateStoreSagaState, CreateStoreAccountErrorModel>
{
    private readonly ITopicProducer<Null, RollbackStoreRequestModel> _producer;
    
    public RollbackBrandActivity(ITopicProducer<Null, RollbackStoreRequestModel> producer)
    {
        _producer = producer;
    }
    public void Probe(ProbeContext context)
    {
        context.CreateScope("RollbackBrandActivity");
    }
    

    public async Task Execute(BehaviorContext<CreateStoreSagaState, CreateStoreAccountErrorModel> context, IBehavior<CreateStoreSagaState, CreateStoreAccountErrorModel> next)
    {
        var rollbackStoreRequestModel = new RollbackStoreRequestModel
        {
            CorrelationId = context.Message.CorrelationId,
            StoreId = context.Message.StoreId,
            AccountId = context.Message.AccountId,
        };
        await _producer.Produce(
            key: null,
            rollbackStoreRequestModel,
            context.CancellationToken);
        await next.Execute(context).ConfigureAwait(false);
    }
    
    public async Task Faulted<TException>(BehaviorExceptionContext<CreateStoreSagaState, CreateStoreAccountErrorModel, TException> context, IBehavior<CreateStoreSagaState, CreateStoreAccountErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);
}
