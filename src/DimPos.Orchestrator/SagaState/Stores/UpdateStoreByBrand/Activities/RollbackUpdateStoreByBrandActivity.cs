using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Store.UpdateStoreByBrand;

namespace DimPos.Orchestrator.SagaState.Stores.UpdateStoreByBrand.Activities;

public class RollbackUpdateStoreByBrandActivity : IStateMachineActivity<UpdateStoreByBrandSagaState, UpdateAccountForStoreByBrandErrorModel>
{
    private readonly ITopicProducer<Null, RollbackUpdateStoreByBrandRequestModel> _topicProducer; 
    
    public RollbackUpdateStoreByBrandActivity(ITopicProducer<Null, RollbackUpdateStoreByBrandRequestModel> topicProducer)
    {
        _topicProducer = topicProducer;
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("RollbackUpdateStoreByBrandActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<UpdateStoreByBrandSagaState, UpdateAccountForStoreByBrandErrorModel> context, IBehavior<UpdateStoreByBrandSagaState, UpdateAccountForStoreByBrandErrorModel> next)
    {
        var rollbackUpdateStoreByBrandRequestModel = new RollbackUpdateStoreByBrandRequestModel
        {
            CorrelationId = context.Message.CorrelationId,
            StoreId = context.Message.StoreId,
            Status = context.Message.Status
        };
        await _topicProducer.Produce(
            key: null,
            rollbackUpdateStoreByBrandRequestModel,
            context.CancellationToken);
        
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdateStoreByBrandSagaState, UpdateAccountForStoreByBrandErrorModel, TException> context, IBehavior<UpdateStoreByBrandSagaState, UpdateAccountForStoreByBrandErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}