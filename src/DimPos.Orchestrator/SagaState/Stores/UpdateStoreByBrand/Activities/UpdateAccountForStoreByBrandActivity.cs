using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Store.UpdateStoreByBrand;

namespace DimPos.Orchestrator.SagaState.Stores.UpdateStoreByBrand.Activities;

public class UpdateAccountForStoreByBrandActivity : IStateMachineActivity<UpdateStoreByBrandSagaState, UpdateStoreByBrandRequestModel>
{
    private readonly ITopicProducer<Null, UpdateAccountForStoreByBrandRequestModel> _producer;
    
    public UpdateAccountForStoreByBrandActivity(ITopicProducer<Null, UpdateAccountForStoreByBrandRequestModel> producer)
    {
        _producer = producer;
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("UpdateAccountForStoreByBrandActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<UpdateStoreByBrandSagaState, UpdateStoreByBrandRequestModel> context, IBehavior<UpdateStoreByBrandSagaState, UpdateStoreByBrandRequestModel> next)
    {
        var updateAccountForStoreByBrandRequestModel = new UpdateAccountForStoreByBrandRequestModel()
        {
            CorrelationId = context.Message.CorrelationId,
            StoreId = context.Message.StoreId,
            BrandAccountId = context.Message.BrandAccountId,
            AccountIds = context.Message.AccountIds,
            Status = context.Message.Status
        };
        await _producer.Produce(
            key: null,
            updateAccountForStoreByBrandRequestModel,
            context.CancellationToken);
        
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<UpdateStoreByBrandSagaState, UpdateStoreByBrandRequestModel, TException> context, IBehavior<UpdateStoreByBrandSagaState, UpdateStoreByBrandRequestModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}