using Confluent.Kafka;
using MassTransit;
using SharedProject.Events.Store.CreateStaff;

namespace DimPos.Orchestrator.SagaState.Stores.CreateStaffSaga.Activities;

public class RollbackStaffStoreAccountActivity : IStateMachineActivity<CreateStaffSagaState, CreateStaffAccountErrorModel>
{
    private readonly ITopicProducer<Null, RollbackStaffStoreAccountRequestModel> _topicProducer;
    
    public RollbackStaffStoreAccountActivity(ITopicProducer<Null, RollbackStaffStoreAccountRequestModel> topicProducer)
    {
        _topicProducer = topicProducer;
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateScope("RollbackStaffStoreAccountActivity");
    }

    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<CreateStaffSagaState, CreateStaffAccountErrorModel> context, IBehavior<CreateStaffSagaState, CreateStaffAccountErrorModel> next)
    {
        var rollbackStaffStoreAccountModel = new RollbackStaffStoreAccountRequestModel
        {
            CorrelationId = context.Message.CorrelationId,
            StoreId = context.Message.StoreId,
            AccountId = context.Message.AccountId
        };

        await _topicProducer.Produce(
            key: null,
            rollbackStaffStoreAccountModel,
            context.CancellationToken);

        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<CreateStaffSagaState, CreateStaffAccountErrorModel, TException> context, IBehavior<CreateStaffSagaState, CreateStaffAccountErrorModel> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}